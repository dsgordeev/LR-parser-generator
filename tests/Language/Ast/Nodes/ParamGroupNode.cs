namespace Language.Ast.Nodes;

public class ParamGroupNode(AstNodeExpression parameterNames, AstNodeExpression groupTypeName) : AstNodeExpression
{
    public AstNodeExpression ParameterNames { get; } = parameterNames;
    public AstNodeExpression GroupTypeName { get; } = groupTypeName;

    public override string ToString() => $"{ParameterNames} of {GroupTypeName}";
}
