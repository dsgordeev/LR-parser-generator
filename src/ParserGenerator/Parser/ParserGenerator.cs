using System.Collections.Generic;
using System.Linq;
using ParserGenerator.Grammar;
using ParserGenerator.Parser.State;

namespace ParserGenerator.Parser;

public class ParserGenerator<TAstNode>(IEnumerable<Production<TAstNode>> productions, Symbol startSymbol)
{
    private readonly Symbol _emptyWord = new("ε", SymbolType.Terminal);
    private readonly Symbol _endOfInput = new("\0", SymbolType.EndOfInput);
    private readonly Symbol _startSymbol = startSymbol;
    private readonly Dictionary<Symbol, HashSet<Symbol>> _firstSets = new();
    private readonly Dictionary<Symbol, HashSet<Symbol>> _followSets = new();
    private readonly List<Production<TAstNode>> _productions = [];
    private readonly Dictionary<Production<TAstNode>, int> _productionIndex = [];
    private readonly Dictionary<Symbol, List<Production<TAstNode>>> _productionsMap = [];
    private readonly List<Symbol> _symbols = [];
    private readonly Dictionary<int, LRState<TAstNode>> _states = [];
    private readonly Dictionary<StateId, LRState<TAstNode>> _statesDict = [];

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
            if (!_productionsMap.TryGetValue(production.Left, out var ps))
            {
                ps = _productionsMap[production.Left] = [];
            }

            ps.Add(production);

            symbols.Add(production.Left);
            foreach (var symbol in production.Right) symbols.Add(symbol);
        }

        // ε ∈ T
        symbols.Add(_emptyWord);

        _symbols.AddRange(symbols);
    }

    /// <summary>
    /// First(A) = { x ∈ T | ∃ A =>* x~ }
    /// A =>* ε ⟹ ε ∈ First(A)
    /// </summary>
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

        // FIRST(ε) = {ε}
        _firstSets[_emptyWord].Add(_emptyWord);

        bool changed;
        do
        {
            changed = false;
            foreach (var production in _productions)
            {
                var left = production.Left;

                // A -> ε ⟹ ε ∈ FIRST(A).
                if (production.Right.Count == 0)
                {
                    if (_firstSets[left].Add(_emptyWord))
                    {
                        changed = true;
                    }

                    continue;
                }

                var rightDeriveEmpty = true;
                // A -> α
                // α = a₁...aₙ
                foreach (var symbol in production.Right)
                {
                    var before = _firstSets[left].Count;

                    // FIRST(aᵢ)\{ε} ⊆ FIRST(A)
                    _firstSets[left].UnionWith(_firstSets[symbol].Where(s => !s.Equals(_emptyWord)));

                    if (_firstSets[left].Count > before)
                    {
                        changed = true;
                    }

                    if (!_firstSets[symbol].Contains(_emptyWord))
                    {
                        rightDeriveEmpty = false;
                        // ε ∈ FIRST(aᵢ) ⟹ FIRST(aᵢ₊₁)\{ε} ⊆ FIRST(A)
                        break;
                    }
                }

                if (rightDeriveEmpty)
                {
                    // α =>* ε ⟹ A =>* ε
                    if (_firstSets[left].Add(_emptyWord))
                    {
                        changed = true;
                    }
                }
            }
        } while (changed);
    }

    /// <summary>
    /// Follow(A) = { x ∈ T | ∃ S =>* ~Ax~ }
    /// </summary>
    private void ComputeFollowSets()
    {
        foreach (var symbol in _symbols.Where(s => s.Type == SymbolType.NonTerminal))
        {
            _followSets[symbol] = [];
        }

        // oef ∈ Follow(S)
        _followSets[_startSymbol].Add(_endOfInput);

        bool changed;
        do
        {
            changed = false;
            // A -> α
            // α = a₁...aₙ
            foreach (var production in _productions)
            {
                var left = production.Left;
                var right = production.Right;
                for (var i = 0; i < right.Count; i++)
                {
                    var current = right[i];
                    if (current.Type != SymbolType.NonTerminal) continue;

                    // aᵢ ∈ N
                    var before = _followSets[current].Count;

                    if (i < right.Count - 1)
                    {
                        // FIRST(beta)\{ ε }
                        // FIRST(aᵢ₊₁...aₙ)\{ ε }
                        var beta = right.Skip(i + 1).ToList();
                        foreach (var symbol in beta)
                        {
                            _followSets[current].UnionWith(
                                _firstSets[symbol].Where(s => !s.Equals(_emptyWord)));

                            if (!_firstSets[symbol].Contains(_emptyWord))
                            {
                                break;
                            }
                        }

                        // aᵢ₊₁...aₙ =>* ε ⟹ FOLLOW(A) ⊆ FOLLOW(aᵢ)
                        if (beta.All(s => _firstSets[s].Contains(_emptyWord)))
                        {
                            _followSets[current].UnionWith(_followSets[left]);
                        }
                    }
                    else
                    {
                        // aᵢ = aₙ ⟹ FOLLOW(A) ⊆ FOLLOW(aᵢ)
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

    /// <summary>
    /// FIRST(beta, a)
    /// </summary>
    private HashSet<Symbol> GetFirst(IEnumerable<Symbol> symbols, Symbol lookahead)
    {
        var first = new HashSet<Symbol>();
        var allDeriveEmpty = true;

        foreach (var symbol in symbols)
        {
            first.UnionWith(_firstSets[symbol].Where(s => !s.Equals(_emptyWord)));

            if (!_firstSets[symbol].Contains(_emptyWord))
            {
                allDeriveEmpty = false;
                break;
            }
        }

        if (allDeriveEmpty)
        {
            // beta =>* ε ⟹ a ∈ FIRST(beta, a)
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

                foreach (var production in _productionsMap[nextSymbol])
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
        // S' -> S
        var augmentedStart = new Symbol(_startSymbol.Name + "'", SymbolType.NonTerminal);
        var augmentedProduction = new Production<TAstNode>(augmentedStart, [_startSymbol]);
        _productions.Insert(0, augmentedProduction);
        _symbols.Add(augmentedStart);
        foreach (var (key, value) in _productions.Select((x, i) => (x, i)).ToDictionary(x => x.x, x => x.i))
        {
            _productionIndex[key] = value;
        }

        // I₀ = Closure([S' -> ·S, eof])
        var initialItem = new LRItem<TAstNode>(augmentedProduction, 0, _endOfInput);
        var initialState = new LRState<TAstNode>(new StateId(0), Closure([initialItem]));
        _states.Add(initialState.GetHashCode(), initialState);
        _statesDict[initialState.Id] = initialState;

        var stateQueue = new Queue<LRState<TAstNode>>();
        stateQueue.Enqueue(initialState);

        // C = {I₀, ..., Iₙ}
        while (stateQueue.Count > 0)
        {
            var currentState = stateQueue.Dequeue();
            foreach (var symbol in _symbols)
            {
                // ε != lookahead
                if (symbol.Equals(_emptyWord)) continue;

                var newItems = Goto(currentState.Items, symbol);
                if (newItems.Count == 0) continue;

                var state = _states.GetValueOrDefault(new LRState<TAstNode>(new StateId(-1), newItems).GetHashCode());
                if (state == null)
                {
                    state = new LRState<TAstNode>(new StateId(_states.Count), newItems);
                    _states.Add(state.GetHashCode(), state);
                    _statesDict[state.Id] = state;
                    stateQueue.Enqueue(state);
                }

                switch (symbol.Type)
                {
                    case SymbolType.Terminal or SymbolType.EndOfInput:
                        _actionTable[(currentState.Id, symbol)] = (ActionType.Shift, state.Id);
                        break;
                    case SymbolType.NonTerminal:
                        _gotoTable[(currentState.Id, symbol)] = state.Id;
                        break;
                }
            }
        }

        foreach (var (_, state) in _states)
        {
            foreach (var item in state.Items.Where(x => x.NextSymbol == null).ToArray())
            {
                if (item.Production.Left.Equals(augmentedStart))
                {
                    // S' -> S·
                    _actionTable[(state.Id, item.Lookahead)] = (ActionType.Accept, new StateId(-1));
                }
                else
                {
                    if (_actionTable.TryGetValue((state.Id, item.Lookahead), out var origin))
                    {
                        var originState = _statesDict[origin.Target];
                        _ = originState.ToString();
                        var replacementState = _statesDict[state.Id];
                        _ = replacementState.ToString();
                    }

                    // A -> α·
                    _actionTable[(state.Id, item.Lookahead)] = (ActionType.Reduce, new StateId(_productionIndex[item.Production]));
                }
            }
        }
    }
}