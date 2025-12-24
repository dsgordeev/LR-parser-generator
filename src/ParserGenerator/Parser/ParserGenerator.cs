using System.Collections.Generic;
using System.Linq;
using ParserGenerator.Grammar;
using ParserGenerator.Parser.State;

namespace ParserGenerator.Parser;

public class ParserGenerator<TAstNode>(IEnumerable<Production<TAstNode>> productions, Symbol startSymbol)
{
    private readonly Symbol _emptyWord = new("ε", SymbolType.Terminal);
    private readonly Symbol _endOfInput = new("$", SymbolType.EndOfInput);
    private readonly Symbol _startSymbol = startSymbol;
    private readonly Dictionary<Symbol, HashSet<Symbol>> _firstSets = new();
    private readonly Dictionary<Symbol, HashSet<Symbol>> _followSets = new();
    private readonly List<Production<TAstNode>> _productions = [];
    private readonly List<Symbol> _symbols = [];
    private readonly List<LRState<TAstNode>> _states = [];
    private readonly Dictionary<(StateId StateId, Symbol Symbol), (ActionType Type, StateId Target)> _actionTable = new();
    private readonly Dictionary<(StateId StateId, Symbol Symbol), StateId> _gotoTable = new();

    public ParserState GenerateState()
    {
        _productions.AddRange(productions);
        InitializeSymbols();
        ComputeFirstSets();
        ComputeFollowSets();
        BuildParser();

        return new ParserState
        {
            StartSymbol = _startSymbol,
            EndOfInput = _endOfInput,
            ActionTable = _actionTable,
            GotoTable = _gotoTable,
        };
    }

    private void InitializeSymbols()
    {
        var symbols = new HashSet<Symbol> { _endOfInput };
        foreach (var production in _productions)
        {
            symbols.Add(production.Left);
            foreach (var symbol in production.Right) symbols.Add(symbol);
        }

        _symbols.AddRange(symbols);
    }

    private void ComputeFirstSets()
    {
        foreach (var symbol in _symbols)
        {
            _firstSets[symbol] = [];
            if (symbol.Type is SymbolType.Terminal or SymbolType.EndOfInput)
            {
                _firstSets[symbol].Add(symbol);
            }
        }

        bool changed;
        do
        {
            changed = false;
            foreach (var production in _productions)
            {
                var left = production.Left;
                foreach (var symbol in production.Right)
                {
                    var before = _firstSets[left].Count;
                    _firstSets[left].UnionWith(_firstSets[symbol]);

                    if (!_firstSets[symbol].Contains(_emptyWord))
                    {
                        break;
                    }

                    if (_firstSets[left].Count > before)
                    {
                        changed = true;
                    }
                }
            }
        } while (changed);
    }

    private void ComputeFollowSets()
    {
        foreach (var symbol in _symbols.Where(s => s.Type == SymbolType.NonTerminal))
        {
            _followSets[symbol] = [];
        }

        _followSets[_startSymbol].Add(_endOfInput);

        bool changed;
        do
        {
            changed = false;
            foreach (var production in _productions)
            {
                var left = production.Left;
                var right = production.Right;
                for (var i = 0; i < right.Count; i++)
                {
                    var current = right[i];
                    if (current.Type != SymbolType.NonTerminal) continue;
                    var before = _followSets[current].Count;

                    if (i < right.Count - 1)
                    {
                        var next = right[i + 1];
                        _followSets[current].UnionWith(_firstSets[next].Where(s => s.Name != "ε"));
                    }

                    if (i == right.Count - 1 || right.Skip(i + 1).All(s => _firstSets[s].Contains(_emptyWord)))
                    {
                        _followSets[current].UnionWith(_followSets[left]);
                    }

                    if (_followSets[current].Count > before)
                    {
                        changed = true;
                    }
                }
            }
        } while (changed);
    }

    private HashSet<Symbol> GetFirst(IEnumerable<Symbol> symbols, Symbol lookahead)
    {
        var first = new HashSet<Symbol>();
        foreach (var symbol in symbols)
        {
            first.UnionWith(_firstSets[symbol].Where(s => s.Name != "ε"));
            if (!_firstSets[symbol].Contains(_emptyWord))
            {
                break;
            }
        }

        if (first.Count == 0)
        {
            first.Add(lookahead);
        }

        return first;
    }

    private HashSet<LRItem<TAstNode>> Goto(HashSet<LRItem<TAstNode>> items, Symbol symbol)
    {
        var newItems = new HashSet<LRItem<TAstNode>>();

        foreach (var item in items)
        {
            if (item.NextSymbol?.Equals(symbol) == true)
            {
                newItems.Add(item.Advance());
            }
        }

        return Closure(newItems);
    }

    private HashSet<LRItem<TAstNode>> Closure(HashSet<LRItem<TAstNode>> items)
    {
        var closure = new HashSet<LRItem<TAstNode>>(items);
        bool changed;
        do
        {
            changed = false;
            foreach (var item in closure.ToList())
            {
                var nextSymbol = item.NextSymbol;
                if (nextSymbol?.Type != SymbolType.NonTerminal)
                {
                    continue;
                }

                var beta = item.Production.Right.Skip(item.Position + 1);
                var lookaheads = GetFirst(beta, item.Lookahead);

                foreach (var production in _productions.Where(p => p.Left.Equals(nextSymbol)))
                {
                    foreach (var lookahead in lookaheads)
                    {
                        if (closure.Add(new LRItem<TAstNode>(production, 0, lookahead)))
                        {
                            changed = true;
                        }
                    }
                }
            }
        } while (changed);

        return closure;
    }

    private void BuildParser()
    {
        // Create augmented grammar
        var augmentedStart = new Symbol(_startSymbol.Name + "'", SymbolType.NonTerminal);
        var augmentedProduction = new Production<TAstNode>(augmentedStart, [_startSymbol]);
        _productions.Insert(0, augmentedProduction);
        _symbols.Add(augmentedStart);

        // Create initial state
        var initialItem = new LRItem<TAstNode>(augmentedProduction, 0, _endOfInput);
        var initialState = new LRState<TAstNode>(new StateId(0), Closure([initialItem]));
        _states.Add(initialState);

        var stateQueue = new Queue<LRState<TAstNode>>();
        stateQueue.Enqueue(initialState);

        while (stateQueue.Count > 0)
        {
            var currentState = stateQueue.Dequeue();
            foreach (var symbol in _symbols)
            {
                var newItems = Goto(currentState.Items, symbol);
                if (newItems.Count == 0) continue;

                var existingState = _states.FirstOrDefault(s => s.Equals(new LRState<TAstNode>(new StateId(-1), newItems)));
                if (existingState != null)
                {
                    switch (symbol.Type)
                    {
                        case SymbolType.Terminal or SymbolType.EndOfInput:
                            _actionTable[(currentState.Id, symbol)] = (ActionType.Shift, existingState.Id);
                            break;
                        case SymbolType.NonTerminal:
                            _gotoTable[(currentState.Id, symbol)] = existingState.Id;
                            break;
                    }
                }
                else
                {
                    var newState = new LRState<TAstNode>(new StateId(_states.Count), newItems);
                    _states.Add(newState);
                    stateQueue.Enqueue(newState);

                    switch (symbol.Type)
                    {
                        case SymbolType.Terminal or SymbolType.EndOfInput:
                            _actionTable[(currentState.Id, symbol)] = (ActionType.Shift, newState.Id);
                            break;
                        case SymbolType.NonTerminal:
                            _gotoTable[(currentState.Id, symbol)] = newState.Id;
                            break;
                    }
                }
            }
        }

        // Add reduce and accept actions
        foreach (var state in _states)
        {
            foreach (var item in state.Items.Where(x => x.NextSymbol == null).ToArray())
            {
                if (item.Production.Left.Equals(augmentedStart))
                {
                    _actionTable[(state.Id, item.Lookahead)] = (ActionType.Accept, new StateId(-1));
                }
                else
                {
                    var productionIndex = _productions.IndexOf(item.Production);

                    _actionTable[(state.Id, item.Lookahead)] = (ActionType.Reduce, new StateId(productionIndex));
                }
            }
        }
    }
}