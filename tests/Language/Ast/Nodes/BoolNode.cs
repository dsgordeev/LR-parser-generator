namespace Language.Ast.Nodes;

public class BoolNode(bool value) : AstNodeExpression
{
    public bool Value { get; } = value;

    public override string ToString() => $"{Value}";
}