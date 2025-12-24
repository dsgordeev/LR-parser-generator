namespace Language.Ast.Nodes;

public class FunctionNode(AstNodeExpression name,
    AstNodeExpression parameterGroups,
    AstNodeExpression resultTypeNames,
    AstNodeExpression body) : AstNodeExpression
{
    public AstNodeExpression Name { get; } = name;
    public AstNodeExpression ParameterGroups { get; } = parameterGroups;
    public AstNodeExpression ResultTypeNames { get; } = resultTypeNames;
    public AstNodeExpression Body { get; } = body;

    public override string ToString() => $"function {Name} takes {ParameterGroups} and returns {Body} as {ResultTypeNames} end function";
}
