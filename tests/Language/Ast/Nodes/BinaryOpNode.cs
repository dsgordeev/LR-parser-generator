namespace Language.Ast.Nodes;

public class BinaryOpNode(string op, AstNodeExpression left, AstNodeExpression right) : AstNodeExpression
{
    public string Operator { get; } = op;
    public AstNodeExpression Left { get; } = left;
    public AstNodeExpression Right { get; } = right;

    public override string ToString() => $"{Left} {Operator} {Right}";
}