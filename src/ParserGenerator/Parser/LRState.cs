using System.Collections.Generic;
using System.Linq;

namespace ParserGenerator.Parser;

public class LRState<TAstNode>(StateId id, IEnumerable<LRItem<TAstNode>> items)
{
    public StateId Id { get; } = id;

    public HashSet<LRItem<TAstNode>> Items { get; } = [..items];

    public override bool Equals(object? obj) => obj is LRState<TAstNode> state && Items.SetEquals(state.Items);
    
    public override int GetHashCode()
    {
        var hash = 17;
        unchecked
        {
            foreach (var item in Items.OrderBy(x => x.GetHashCode()))
            {
                hash = hash * 23 + item.GetHashCode();
            }
        }

        return hash;
    }

    public override string ToString() => $"State {Id}:\r\n{string.Join("\r\n", Items)}";
}