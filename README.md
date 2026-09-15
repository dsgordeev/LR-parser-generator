# LR-parser-generator
Pure C# LR(1) parser generator for .NET

### How to use
```csharp
var parserStateGenerator = new ParserGenerator<AstNode>(Grammar.Productions, Grammar.Start);
var parserState = parserStateGenerator.GenerateState();
var parser = new Parser<AstNode>(
    parserState,
    Grammar.Productions.ToList(),
    new Parser<AstNode>.ParserOptions(token => token.Value switch
    {
        double num => new AstNode.Num(num),
        long   num => new AstNode.Num(num),
        string     => new AstNode.Num(-1),
        _ => throw new ArgumentOutOfRangeException($"Unexpected token {token}")
    }));

var ast = parser.Parse(new LanguageLexer().Reset("1 + 2 * 3").Tokenize());

// ast ~ ([1] + ([2] * [3]))

public static class Grammar
{
    public static Symbol NUM     = new(nameof(NUM), SymbolType.Terminal);
    public static Symbol PLUS    = new("+", SymbolType.Terminal);
    public static Symbol MUL     = new("*", SymbolType.Terminal);
    public static Symbol LP      = new("(", SymbolType.Terminal);
    public static Symbol RP      = new(")", SymbolType.Terminal);

    public static Symbol Program = new(nameof(Program), SymbolType.NonTerminal);
    public static Symbol Expr    = new(nameof(Expr),    SymbolType.NonTerminal);
    public static Symbol Term    = new(nameof(Term),    SymbolType.NonTerminal);
    public static Symbol Factor  = new(nameof(Factor),  SymbolType.NonTerminal);

    public static Production<AstNode>[] Productions =
    [
        new(Program, [Expr]),

        new(Expr, [Expr, PLUS, Term],  vs => new AstNode.Binary("+", vs[0], vs[2])),
        new(Expr, [Term]),

        new(Term, [Term, MUL, Factor], vs => new AstNode.Binary("*", vs[0], vs[2])),
        new(Term, [Factor]),

        new(Factor, [LP, Expr, RP],    vs => vs[1]),
        new(Factor, [NUM])
    ];

    public static Symbol Start => Program;
}

public abstract class AstNode : IWithPosition<AstNode>
{
    public Position Position { get; private set; }

    public AstNode WithPosition(IList<AstNode> list) { /* Position = f(list) */ return this; }

    public class Num(object value) : AstNode;

    public class Binary(string op, AstNode left, AstNode right) : AstNode;
}
```