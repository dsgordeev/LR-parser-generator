namespace Language.Ast.Nodes;

public class UnaryOpNode(string op, AstNodeExpression operand) : AstNodeExpression
{
    public string Operator { get; } = op;
    public AstNodeExpression Operand { get; } = operand;

    public override string ToString() => $"{Operator}{Operand}";
}