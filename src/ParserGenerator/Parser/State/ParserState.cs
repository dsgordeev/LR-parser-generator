using System.Collections.Generic;
using ParserGenerator.Grammar;

namespace ParserGenerator.Parser.State;

public sealed record ParserState
{
    public int Version { get; set; }

    public Symbol StartSymbol { get; set; } = null!;

    public Symbol EndOfInput { get; set; } = null!;

    public Dictionary<(StateId StateId, Symbol Symbol), (ActionType Type, StateId Target)> ActionTable { get; set; } = [];

    public Dictionary<(StateId StateId, Symbol Symbol), StateId> GotoTable { get; set; } = [];
}