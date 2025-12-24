using System.Collections.Generic;
using Language.Ast;
using Language.Ast.Nodes;
using ParserGenerator.Grammar;

namespace Language.Grammar;

public class LanguageGrammar
{
    // Terminal symbols
    // ReSharper disable InconsistentNaming
    public static readonly Symbol ID = new(nameof(ID), SymbolType.Terminal);
    public static readonly Symbol NUM = new(nameof(NUM), SymbolType.Terminal);
    public static readonly Symbol TRUE = new(nameof(TRUE).ToLowerInvariant(), SymbolType.Terminal);
    public static readonly Symbol FALSE = new(nameof(FALSE).ToLowerInvariant(), SymbolType.Terminal);
    public static readonly Symbol PLUS = new("+", SymbolType.Terminal);
    public static readonly Symbol MINUS = new("-", SymbolType.Terminal);
    public static readonly Symbol MUL = new("*", SymbolType.Terminal);
    public static readonly Symbol DIV = new("/", SymbolType.Terminal);
    public static readonly Symbol POWER = new("**", SymbolType.Terminal);
    public static readonly Symbol MOD = new("%", SymbolType.Terminal);
    public static readonly Symbol EQ = new("=", SymbolType.Terminal);
    public static readonly Symbol NEQ = new("!=", SymbolType.Terminal);
    public static readonly Symbol LT = new("<", SymbolType.Terminal);
    public static readonly Symbol GT = new(">", SymbolType.Terminal);
    public static readonly Symbol LTE = new("<=", SymbolType.Terminal);
    public static readonly Symbol GTE = new(">=", SymbolType.Terminal);
    public static readonly Symbol AND = new("&", SymbolType.Terminal);
    public static readonly Symbol OR = new("|", SymbolType.Terminal);
    public static readonly Symbol XOR = new("^", SymbolType.Terminal);
    public static readonly Symbol CONCAT = new("||", SymbolType.Terminal);
    public static readonly Symbol NOT = new("!", SymbolType.Terminal);
    public static readonly Symbol IF = new(nameof(IF), SymbolType.Terminal);
    public static readonly Symbol THEN = new(nameof(THEN), SymbolType.Terminal);
    public static readonly Symbol ELSE = new(nameof(ELSE), SymbolType.Terminal);
    public static readonly Symbol ELSEIF = new(nameof(ELSEIF), SymbolType.Terminal);
    public static readonly Symbol END = new(nameof(END), SymbolType.Terminal);
    public static readonly Symbol RETURNS = new(nameof(RETURNS), SymbolType.Terminal);
    public static readonly Symbol LPAREN = new("(", SymbolType.Terminal);
    public static readonly Symbol RPAREN = new(")", SymbolType.Terminal);
    public static readonly Symbol LBRACE = new("{", SymbolType.Terminal);
    public static readonly Symbol RBRACE = new("}", SymbolType.Terminal);
    public static readonly Symbol LBRACKET = new("[", SymbolType.Terminal);
    public static readonly Symbol RBRACKET = new("]", SymbolType.Terminal);
    public static readonly Symbol COMMA = new(",", SymbolType.Terminal);
    public static readonly Symbol SEMI = new(";", SymbolType.Terminal);
    public static readonly Symbol COLON = new(":", SymbolType.Terminal);
    public static readonly Symbol DOT = new(".", SymbolType.Terminal);
    public static readonly Symbol FUNCTION = new(nameof(FUNCTION), SymbolType.Terminal);
    public static readonly Symbol INTEGER = new(nameof(INTEGER), SymbolType.Terminal);
    public static readonly Symbol REAL = new(nameof(REAL), SymbolType.Terminal);
    public static readonly Symbol DOUBLE = new(nameof(DOUBLE), SymbolType.Terminal);
    public static readonly Symbol STRING = new(nameof(STRING), SymbolType.Terminal);
    public static readonly Symbol CHAR = new(nameof(CHAR), SymbolType.Terminal);
    public static readonly Symbol BOOLEAN = new(nameof(BOOLEAN), SymbolType.Terminal);
    public static readonly Symbol EOF = new("$", SymbolType.EndOfInput);
    // ReSharper restore InconsistentNaming

    public static readonly Dictionary<string, Symbol> KeywordSymbols = new()
    {
        ["function"] = FUNCTION,
        ["if"] = IF,
        ["then"] = THEN,
        ["else"] = ELSE,
        ["elseif"] = ELSEIF,
        ["end"] = END,
        ["returns"] = RETURNS,
        ["true"] = TRUE,
        ["false"] = FALSE,
        ["integer"] = INTEGER,
        ["real"] = REAL,
        ["double"] = DOUBLE,
        ["string"] = STRING,
        ["char"] = CHAR,
        ["boolean"] = BOOLEAN
    };

    // Non-terminal symbols
    public static readonly Symbol Program = new(nameof(Program), SymbolType.NonTerminal);
    public static readonly Symbol ExprList = new(nameof(ExprList), SymbolType.NonTerminal);
    public static readonly Symbol Expr = new(nameof(Expr), SymbolType.NonTerminal);
    public static readonly Symbol Expr1 = new(nameof(Expr1), SymbolType.NonTerminal);
    public static readonly Symbol Expr2 = new(nameof(Expr2), SymbolType.NonTerminal);
    public static readonly Symbol Expr3 = new(nameof(Expr3), SymbolType.NonTerminal);
    public static readonly Symbol Expr4 = new(nameof(Expr4), SymbolType.NonTerminal);
    public static readonly Symbol Expr5 = new(nameof(Expr5), SymbolType.NonTerminal);
    public static readonly Symbol Expr6 = new(nameof(Expr6), SymbolType.NonTerminal);
    public static readonly Symbol Expr7 = new(nameof(Expr7), SymbolType.NonTerminal);
    public static readonly Symbol Expr8 = new(nameof(Expr8), SymbolType.NonTerminal);
    public static readonly Symbol Expr9 = new(nameof(Expr9), SymbolType.NonTerminal);
    public static readonly Symbol Expr10 = new(nameof(Expr10), SymbolType.NonTerminal);
    public static readonly Symbol BoolExpr = new(nameof(BoolExpr), SymbolType.NonTerminal);
    public static readonly Symbol Term = new(nameof(Term), SymbolType.NonTerminal);
    public static readonly Symbol Factor = new(nameof(Factor), SymbolType.NonTerminal);
    public static readonly Symbol IfExpr = new(nameof(IfExpr), SymbolType.NonTerminal);
    public static readonly Symbol ElseIfClauses = new(nameof(ElseIfClauses), SymbolType.NonTerminal);
    public static readonly Symbol ElseIfClause = new(nameof(ElseIfClause), SymbolType.NonTerminal);
    public static readonly Symbol FunctionList = new(nameof(FunctionList), SymbolType.NonTerminal);
    public static readonly Symbol Function = new(nameof(Function), SymbolType.NonTerminal);
    public static readonly Symbol TypeList = new(nameof(TypeList), SymbolType.NonTerminal);
    public static readonly Symbol ParamGroupList = new(nameof(ParamGroupList), SymbolType.NonTerminal);
    public static readonly Symbol ParamGroup = new(nameof(ParamGroup), SymbolType.NonTerminal);
    public static readonly Symbol ParamNames = new(nameof(ParamNames), SymbolType.NonTerminal);
    public static readonly Symbol PostfixOp = new(nameof(PostfixOp), SymbolType.NonTerminal);

    public static readonly Symbol TypeValue = new(nameof(TypeValue), SymbolType.NonTerminal);
    public static readonly Symbol PrimitiveType = new(nameof(PrimitiveType), SymbolType.NonTerminal);
    public static readonly Symbol FunctionType = new(nameof(FunctionType), SymbolType.NonTerminal);

    public static Production<AstNodeExpression>[] Productions =>
[
    new(Program, [FunctionList],
        vs => new ModuleNode(null, null!, null!, vs[0])),

    new(Function, [FUNCTION, ID, LPAREN, ParamGroupList, RETURNS, TypeList, RPAREN, ExprList, END, FUNCTION],
        vs => new FunctionNode(vs[1], vs[3], vs[5], vs[7])),

    new(FunctionList, [Function]),
    new(FunctionList, [FunctionList, Function],
        vs => new ExprListNode([..vs[0].AsList(), vs[1]])),

    new(ParamGroup, [ParamNames, COLON, TypeValue],
        vs => new ParamGroupNode(vs[0], vs[2])),

    new(ParamGroupList, [ParamGroup]),
    new(ParamGroupList, [ParamGroupList, SEMI, ParamGroup],
        vs => new ExprListNode([vs[0], ..vs[2].AsList()])),

    new(ParamNames, [ID]),
    new(ParamNames, [ID , COMMA, ParamNames],
        vs => new ExprListNode([vs[0], ..vs[2].AsList()])),

    new(TypeValue, [PrimitiveType]),
    new(TypeValue, [FunctionType]),
    new(TypeValue, [ID]),

    new(PrimitiveType, [INTEGER], vs => new TypeNode(vs[0])),
    new(PrimitiveType, [REAL], vs => new TypeNode(vs[0])),
    new(PrimitiveType, [DOUBLE], vs => new TypeNode(vs[0])),
    new(PrimitiveType, [CHAR], vs => new TypeNode(vs[0])),
    new(PrimitiveType, [BOOLEAN], vs => new TypeNode(vs[0])),
    new(PrimitiveType, [STRING], vs => new TypeNode(vs[0])),

    new(FunctionType, [FUNCTION, LBRACKET, RETURNS, TypeList, RBRACKET], vs => new TypeNode.FunctionTypeNode(null, null, vs[3])),
    new(FunctionType, [FUNCTION, LBRACKET, TypeList, RETURNS, TypeList, RBRACKET], vs => new TypeNode.FunctionTypeNode(null, vs[2], vs[4])),

    new(TypeList, [TypeValue]),
    new(TypeList, [TypeValue, COMMA, TypeList],
        vs => new ExprListNode([vs[0], ..vs[2].AsList()])),
    
    // Expressions
    // Starting with the lowest priority 1: concatenation (||) - left associative
    new(Expr, [Expr, CONCAT, Expr1],
        vs => new BinaryOpNode("||", vs[0], vs[2])),
    new(Expr, [Expr1]),
    
    // Priority 2: OR (|) - left associative
    new(Expr1, [Expr1, OR, Expr2],
        vs => new BinaryOpNode("|", vs[0], vs[2])),
    new(Expr1, [Expr2]),
    
    // Priority 3: XOR (^) - left associative
    new(Expr2, [Expr2, XOR, Expr3],
        vs => new BinaryOpNode("^", vs[0], vs[2])),
    new(Expr2, [Expr3]),
    
    // Priority 4: AND (&) - left associative
    new(Expr3, [Expr3, AND, Expr4],
        vs => new BinaryOpNode("&", vs[0], vs[2])),
    new(Expr3, [Expr4]),
    
    //// Priority 5: = and != - left associative
    new(Expr4, [Expr4, EQ, Expr5],
        vs => new BinaryOpNode("==", vs[0], vs[2])),
    new(Expr4, [Expr4, NEQ, Expr5],
        vs => new BinaryOpNode("!=", vs[0], vs[2])),
    new(Expr4, [Expr5]),
    
    // Priority 6: <, >, <=, >= - left associative
    new(Expr5, [Expr5, LT, Expr6],
        vs => new BinaryOpNode("<", vs[0], vs[2])),
    new(Expr5, [Expr5, GT, Expr6],
        vs => new BinaryOpNode(">", vs[0], vs[2])),
    new(Expr5, [Expr5, LTE, Expr6],
        vs => new BinaryOpNode("<=", vs[0], vs[2])),
    new(Expr5, [Expr5, GTE, Expr6],
        vs => new BinaryOpNode(">=", vs[0], vs[2])),
    new(Expr5, [Expr6]),
    
    // Priority 7: + and - - left associative
    new(Expr6, [Expr6, PLUS, Expr7],
        vs => new BinaryOpNode("+", vs[0], vs[2])),
    new(Expr6, [Expr6, MINUS, Expr7],
        vs => new BinaryOpNode("-", vs[0], vs[2])),
    new(Expr6, [Expr7]),
    
    // Priority 8: *, /, % - left associative
    new(Expr7, [Expr7, MUL, Expr8],
        vs => new BinaryOpNode("*", vs[0], vs[2])),
    new(Expr7, [Expr7, DIV, Expr8],
        vs => new BinaryOpNode("/", vs[0], vs[2])),
    new(Expr7, [Expr7, MOD, Expr8],
        vs => new BinaryOpNode("%", vs[0], vs[2])),
    new(Expr7, [Expr8]),

    // Priority 9: ** (power) - right associative
    new(Expr8, [Expr9, POWER, Expr8],
        vs => new BinaryOpNode("**", vs[0], vs[2])),
    new(Expr8, [Expr9]),
    
    // Unary operators: +, -, ! (highest priority)
    new(Expr9, [MINUS, Expr9],
        vs => new UnaryOpNode("-", vs[1])),
    new(Expr9, [NOT, Expr9],
        vs => new UnaryOpNode("!", vs[1])),
    new(Expr9, [Expr10]),
    
    // Atomic expressions
    new(Expr10, [TRUE], _ => new BoolNode(true)),
    new(Expr10, [FALSE], _ => new BoolNode(false)),
    new(Expr10, [STRING]),
    new(Expr10, [NUM]),
    new(Expr10, [ID]),
    
    // Parenthesized expressions
    new(Expr10, [LPAREN, Expr, RPAREN], vs => vs[1].WithWrap()),
    
    // Postfix operations
    new(Expr10, [Expr10, PostfixOp],
        vs => new PostfixNode(vs[0], vs[1])),

    // Postfix operation types
    new(PostfixOp, [LPAREN, ExprList, RPAREN],
        vs => new PostfixNode.FunctionCallNode(vs[1])),
    new(PostfixOp, [LPAREN, RPAREN],
        vs => new PostfixNode.FunctionCallNode(null)),
    new(PostfixOp, [COLON, PrimitiveType],
        vs => new PostfixNode.TypeCastNode(vs[1])),

    // Expression list
    new(ExprList, [Expr]),
    new(ExprList, [Expr, COMMA, ExprList],
        vs => new ExprListNode([vs[0], ..vs[2].AsList()])),

    // Other expressions (If, LetIn, For loops)
    new(Expr, [IfExpr]),
    new(Expr, [FUNCTION, ID, LPAREN, ParamGroupList, RETURNS, TypeList, RPAREN, ExprList, END, FUNCTION],
        vs => new FunctionNode(vs[1], vs[3], vs[5], vs[7])),

    // IfExpr
    new(IfExpr, [IF, Expr, THEN, ExprList, ELSE, ExprList, END, IF],
        vs => new IfNode(vs[1], vs[3], vs[5], null)),
    new(IfExpr, [IF, Expr, THEN, ExprList, ElseIfClauses, ELSE, ExprList, END, IF],
        vs => new IfNode(vs[1], vs[3], vs[6], vs[4])),

    new(ElseIfClause, [ELSEIF, Expr, THEN, ExprList],
        vs => new IfNode.ElseIfNode(vs[1], vs[3])),

    // IfExpr.ElseIf productions
    new (ElseIfClauses, [ElseIfClause]),
    new (ElseIfClauses, [ElseIfClauses, ElseIfClause],
        vs => new ExprListNode([..vs[0].AsList(), ..vs[1].AsList()])),

];

    public static Symbol Start => Program;
}