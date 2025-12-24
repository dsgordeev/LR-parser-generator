namespace Language.Ast.Nodes;

public class ModuleNode(AstNodeExpression? name, AstNodeExpression? modules, AstNodeExpression? types, AstNodeExpression functions) : AstNodeExpression
{
    public AstNodeExpression? Name { get; } = name;
    public AstNodeExpression? Modules { get; } = modules;
    public AstNodeExpression? Types { get; } = types;
    public AstNodeExpression Functions { get; } = functions;

    public override string ToString()
    {
        return $"module [{Name}] uses [{Modules}] with [{Types}] implements [{Functions}]";
    }
}
