namespace ParserGenerator.Positions;

public record struct Position(int StartRow, int StartCol, int EndRow, int EndCol)
{
    public override string ToString()
    {
        return $"({StartRow},{StartCol})-({EndRow},{EndCol})";
    }
}