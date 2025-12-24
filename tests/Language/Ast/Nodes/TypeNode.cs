namespace Language.Ast.Nodes;

public class TypeNode(AstNodeExpression name) : AstNodeExpression
{
    public AstNodeExpression Name { get; } = name;

    public override string ToString() => $"{Name}";

    public class FunctionTypeNode(AstNodeExpression? name, AstNodeExpression? inTypes, AstNodeExpression outTypes) : AstNodeExpression
    {
        public AstNodeExpression? Name { get; } = name;
        public AstNodeExpression? InTypes { get; } = inTypes;
        public AstNodeExpression OutTypes { get; } = outTypes;

        public override string ToString() => $"lambda {Name} {InTypes} -> {OutTypes}";
    }
}