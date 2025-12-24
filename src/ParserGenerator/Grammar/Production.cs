using System;
using System.Collections.Generic;

namespace ParserGenerator.Grammar;

public class Production<TAstNode>(Symbol left, IEnumerable<Symbol> right, Func<TAstNode[], TAstNode>? reduceAction = null)
{
    public Symbol Left { get; } = left;
    public List<Symbol> Right { get; } = [..right];
    public Func<TAstNode[], TAstNode>? ReduceAction { get; } = reduceAction;

    public override string ToString() => $"{Left} -> {string.Join(" ", Right)}";
}