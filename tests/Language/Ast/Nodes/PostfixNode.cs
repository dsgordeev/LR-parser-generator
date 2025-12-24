namespace Language.Ast.Nodes;

public class PostfixNode(AstNodeExpression factor, AstNodeExpression postfixOperator) : AstNodeExpression
{
    public AstNodeExpression Factor { get; } = factor;
    public AstNodeExpression PostfixOperator { get; } = postfixOperator;

    public override string ToString() => $"{Factor}{PostfixOperator}";


    public class FunctionCallNode(AstNodeExpression? parameters) : AstNodeExpression
    {
        public AstNodeExpression? Parameters { get; } = parameters;

        public override string ToString() => $"({Parameters})";
    }

    public class TypeCastNode(AstNodeExpression targetType) : AstNodeExpression
    {
        public AstNodeExpression TargetType { get; } = targetType;

        public override string ToString() => $":{TargetType}";
    }
}