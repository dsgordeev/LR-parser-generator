using System;
using System.Linq;
using ParserGenerator.Grammar;

namespace ParserGenerator.Parser;

public class LRItem<TAstNode>(Production<TAstNode> production, int position, Symbol lookahead)
{
    public Production<TAstNode> Production { get; } = production;
    public int Position { get; } = position;
    public Symbol Lookahead { get; } = lookahead;

    public Symbol? NextSymbol => Position < Production.Right.Count ? Production.Right[Position] : null;

    public LRItem<TAstNode> Advance() => new(Production, Position + 1, Lookahead);

    public override bool Equals(object? obj) => obj is LRItem<TAstNode> item 
                                                && Production == item.Production
                                                && Position == item.Position 
                                                && Lookahead.Name.Equals(item.Lookahead.Name, StringComparison.OrdinalIgnoreCase)
                                                && Lookahead.Type == item.Lookahead.Type
                                                ;

    public override int GetHashCode()
    {
        var hash = 17;

        unchecked
        {
            hash = hash * 23 + Production.GetHashCode();
            hash = hash * 23 + Position.GetHashCode();
            hash = hash * 23 + Lookahead.Name.ToLowerInvariant().GetHashCode();
            hash = hash * 23 + Lookahead.Type.GetHashCode();
        }

        return hash;
    }

    public override string ToString() => $"{Production.Left} -> {string.Join(" ", Production.Right.Take(Position))}•{string.Join(" ", Production.Right.Skip(Position))}, {Lookahead}";
}