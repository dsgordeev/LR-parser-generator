using System.Text;
using Language.Ast;
using Language.Ast.Algorithms.Convert;
using Language.Ast.Nodes;
using Language.Grammar;
using Language.LexicalAnalysis;
using ParserGenerator.Parser;
using ParserGenerator.Parser.State;

namespace Language.Tests;

[TestClass]
public sealed class ParserTests
{
    private static ParserState _parserState = null!;
    private static Parser<AstNodeExpression>.ParserOptions _options = null!;

    private static readonly Dictionary<string, string> AstMap = [];
    private static readonly Dictionary<string, string> CsMap = [];
    private static readonly Dictionary<string, string> CppMap = [];

    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context)
    {
        var parserStateGenerator = new ParserGenerator<AstNodeExpression>(LanguageGrammar.Productions, LanguageGrammar.Start);
        _parserState = parserStateGenerator.GenerateState();

        _options = new Parser<AstNodeExpression>.ParserOptions(token => token.Value switch
        {
            double num => new NumberNode(num).WithPosition(token.Position),
            long num => new NumberNode(num).WithPosition(token.Position),
            string id => new IdentifierNode(id).WithPosition(token.Position),
            _ => throw new ArgumentOutOfRangeException($"Unexpected token {token}")
        })
        {
            // Out = Console.Out
        };
    }

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        const string arith =
            """

            function Main(X, Y1, Y2, Y3: integer returns integer, integer)
                X * ( Y1 + Y2 ) , X || Y1 | Y2 ^ Y3 & X = Y1 + Y2 * -Y3 ** 1
            end  function

            """;

        AstMap[arith] = "module [] uses [] with [] implements [function \"Main\" takes  \"X\", \"Y1\", \"Y2\", \"Y3\" of \"integer\" and returns  \"X\" * \"Y1\" + \"Y2\", \"X\" || \"Y1\" | \"Y2\" ^ \"Y3\" & \"X\" == \"Y1\" + \"Y2\" * -\"Y3\" ** 1 as  \"integer\", \"integer\" end function]";
        CsMap[arith] = "public static void Main ( int   X ,  int   Y1 ,  int   Y2 ,  int   Y3 , out  int  y1, out  int  y2)\r\n{\r\n    y1 = ( X  * ( Y1  +  Y2 ));\r\n    y2 = ( X  || ( Y1  || ( Y2  ^ ( Y3  && ( X  == ( Y1  + ( Y2  * (- Y3  **  1 ))))))));\r\n}\r\n";
        CppMap[arith] = 
            """


            static std::tuple<int, int> Main(int X, int Y1, int Y2, int Y3)
            {
                return std::make_tuple((X * (Y1 + Y2)), (X || (Y1 | (Y2 ^ (Y3 & (X == (Y1 + (Y2 * ((-Y3) ** 1)))))))));
            }

            """;

        const string if_then_else =
            """

            function Main(X, Y1, Y2, Y3: integer returns integer, integer)
                if X > 100 & true then 
                   X, 1001
                elseif X > 50 then 
                   Y1, 1002
                elseif X > 25 then 
                   Y2, 1003
                else 
                   Y3, 1004
                end if
            end function

            """;
        AstMap[if_then_else] = "module [] uses [] with [] implements [function \"Main\" takes  \"X\", \"Y1\", \"Y2\", \"Y3\" of \"integer\" and returns if \"X\" > 100 & True then  \"X\", 1001  elseif \"X\" > 50 then  \"Y1\", 1002,  elseif \"X\" > 25 then  \"Y2\", 1003 else  \"Y3\", 1004 end if as  \"integer\", \"integer\" end function]";
        CsMap[if_then_else] = "public static void Main ( int   X ,  int   Y1 ,  int   Y2 ,  int   Y3 , out  int  y1, out  int  y2)\r\n{\r\n    y1 = ((( X  >  100 ) &&  true )) ?  X  1001 (( X  >  50 ))  :  Y1  1002 (( X  >  25 ))  :  Y2  1003  :  Y3  1004 ;\r\n}\r\n";
        CppMap[if_then_else] = 
            """


            static std::tuple<int, int> Main(int X, int Y1, int Y2, int Y3)
            {
                return (((X > 100) & true) ? std::make_tuple(X, 1001) : (X > 50) ? std::make_tuple(Y1, 1002) : (X > 25) ? std::make_tuple(Y2, 1003) : std::make_tuple(Y3, 1004));
            }

            """;

        const string call =
            """
            function min(X, Y, Z, T, U: integer returns integer)
                U
            end function
            
            function Main(X, Y1, Y2, Y3: integer returns integer, integer)
                min(X, Y1, Y2, Y3, Y2 - Y3)
            end function

            """;

        AstMap[call] = "module [] uses [] with [] implements [ function \"min\" takes  \"X\", \"Y\", \"Z\", \"T\", \"U\" of \"integer\" and returns \"U\" as \"integer\" end function, function \"Main\" takes  \"X\", \"Y1\", \"Y2\", \"Y3\" of \"integer\" and returns \"min\"( \"X\", \"Y1\", \"Y2\", \"Y3\", \"Y2\" - \"Y3\") as  \"integer\", \"integer\" end function]";
        CsMap[call] = "public static void min ( int   X ,  int   Y ,  int   Z ,  int   T ,  int   U , int )\r\n{\r\n    y1 =  U ;\r\n}\r\npublic static void Main ( int   X ,  int   Y1 ,  int   Y2 ,  int   Y3 , out  int  y1, out  int  y2)\r\n{\r\n    y1 =     |!PostfixNode!|;\r\n}\r\n";
        CppMap[call] =
            """
            
            std::make_tuple(
                static int min(int X, int Y, int Z, int T, int U)
            {
                    return U;
            }
            , 
                static std::tuple<int, int> Main(int X, int Y1, int Y2, int Y3)
            {
                    return min(std::make_tuple(X, Y1, Y2, Y3, (Y2 - Y3)));
            }
            )
            """;
    }

    [TestMethod]
    [DynamicData(nameof(GetTestData), DynamicDataSourceType.Method)]
    public void Parse_WhenExpression_ReturnsAst(string input, string expected, string csTranslation, string cppTranslation)
    {
        // Arrange
        var lexer = new LanguageLexer();
        var parser = new Parser<AstNodeExpression>(_parserState, [.. LanguageGrammar.Productions], _options);

        // Act
        var ast = parser.Parse(lexer.Reset(input).Tokenize());

        // Assert
        Assert.AreEqual(expected, ast.ToString());
    }

    [TestMethod]
    [DynamicData(nameof(GetTestData), DynamicDataSourceType.Method)]
    public void Convert_WhenExpression_ReturnsCsCode(string input, string expected, string csTranslation, string cppTranslation)
    {
        // Arrange
        var lexer = new LanguageLexer();
        var parser = new Parser<AstNodeExpression>(_parserState, [.. LanguageGrammar.Productions], _options);
        var ast = parser.Parse(lexer.Reset(input).Tokenize());

        // Act
        var csResult = Convert(ast, x => new CsTranslatorVisitor(x));

        // Assert
        Assert.AreEqual(csTranslation, csResult);
    }

    [TestMethod]
    [DynamicData(nameof(GetTestData), DynamicDataSourceType.Method)]
    public void Convert_WhenExpression_ReturnsCppCode(string input, string expected, string csTranslation, string cppTranslation)
    {
        // Arrange
        var lexer = new LanguageLexer();
        var parser = new Parser<AstNodeExpression>(_parserState, [.. LanguageGrammar.Productions], _options);
        var ast = parser.Parse(lexer.Reset(input).Tokenize());

        // Act
        var cppResult = Convert(ast, x => new CppTranslatorVisitor(x));

        // Assert
        Assert.AreEqual(cppTranslation, cppResult);
    }
    
    private static string Convert(AstNodeExpression astNode, Func<TextWriter, AstNodeExpressionVisitor> visitor)
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        visitor(writer).Visit(astNode);
        return sb.ToString();
    }

    private static IEnumerable<object[]> GetTestData()
    {
        foreach (var (input, astNode) in AstMap)
        {
            yield return [input, astNode, CsMap[input], CppMap[input]];
        }
    }
}