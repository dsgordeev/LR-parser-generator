using ParserGenerator.Grammar;
using ParserGenerator.Positions;

namespace ParserGenerator.Lexer;

public record Token(Symbol Symbol, object Value, Position Position) : IWithPosition
{
    public Symbol Symbol { get; } = Symbol;

    public object? Value { get; } = Value;

    public Position Position { get; } = Position;
}
