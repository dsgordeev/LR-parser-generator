namespace ParserGenerator.Parser;

public record struct StateId(int Id)
{
    public override string ToString() => Id.ToString();
}