namespace Language.Ast.Nodes;

public class IdentifierNode(string name) : AstNodeExpression
{
    public string Name { get; } = name;
    
    public override string ToString() => $"\"{Name}\"";
}