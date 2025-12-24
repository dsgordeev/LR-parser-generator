namespace ParserGenerator.Grammar;

public record Symbol(string Name, SymbolType Type)
{
    public string Name { get; } = Name;
    public SymbolType Type { get; } = Type;

    public override string ToString() => Name;
}
