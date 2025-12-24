using System.Collections.Generic;

namespace ParserGenerator.Positions;

public interface IWithPosition<T> : IWithPosition
{
    T WithPosition(IList<T> items);
}

public interface IWithPosition
{
    Position Position { get; }
}