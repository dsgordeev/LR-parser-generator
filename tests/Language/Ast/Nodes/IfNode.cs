namespace Language.Ast.Nodes;

public class IfNode(
    AstNodeExpression test,
    AstNodeExpression thenBranch,
    AstNodeExpression elseBranch,
    AstNodeExpression? elseIfs)
    : AstNodeExpression
{
    public AstNodeExpression Test { get; } = test;
    public AstNodeExpression ThenBranch { get; } = thenBranch;
    public AstNodeExpression ElseBranch { get; } = elseBranch;
    public AstNodeExpression? ElseIfs { get; } = elseIfs;

    public override string ToString() => $"if {Test} then {ThenBranch}{ElseIfs} else {ElseBranch} end if";

    public class ElseIfNode(AstNodeExpression condition, AstNodeExpression body) : AstNodeExpression
    {
        public AstNodeExpression Test { get; } = condition;
        public AstNodeExpression Body { get; } = body;

        public override string ToString() => $" elseif {Test} then {Body}";
    }
}