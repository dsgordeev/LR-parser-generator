using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ParserGenerator.Grammar;
using ParserGenerator.Lexer;
using ParserGenerator.Parser.State;
using ParserGenerator.Positions;

namespace ParserGenerator.Parser;

public sealed class Parser<TAstNode> where TAstNode : IWithPosition<TAstNode>
{
    private readonly ParserOptions _options;

    private readonly Symbol _endOfInput;
    private readonly List<Production<TAstNode>> _productions;
    private readonly Dictionary<(StateId StateId, Symbol Symbol), (ActionType Type, StateId Target)> _actionTable;
    private readonly Dictionary<(StateId StateId, Symbol Symbol), StateId> _gotoTable;

    public Parser(ParserState state, List<Production<TAstNode>> productions, ParserOptions options)
    {
        _options = options;

        var startSymbol = state.StartSymbol;

        _endOfInput = state.EndOfInput;
        _productions = productions;
        _actionTable = state.ActionTable;
        _gotoTable = state.GotoTable;

        if (_productions.FirstOrDefault()?.Left.Name != startSymbol.Name + "'")
        {
            var augmentedStart = new Symbol(startSymbol.Name + "'", SymbolType.NonTerminal);
            var augmentedProduction = new Production<TAstNode>(augmentedStart, [startSymbol]);
            _productions.Insert(0, augmentedProduction);
        }
    }

    public TAstNode Parse(IEnumerable<Token> inputTokens)
    {
        var valueStack = new Stack<IWithPosition>();
        var stateStack = new Stack<StateId>();
        stateStack.Push(new StateId(0)); // Initial state
        using var tokens = new LazyEnumerable<Token>(inputTokens, new Token(_endOfInput, null!, default));

        while (tokens.NotEmpty())
        {
            var currentState = stateStack.Peek();
            var currentToken = tokens.Peek();

            if (!_actionTable.TryGetValue((currentState, currentToken.Symbol), out var action))
            {
                action = (ActionType.Error, new StateId(-1));
            }

            switch (action.Type)
            {
                case ActionType.Shift:
                    valueStack.Push(currentToken); // Push token value
                    stateStack.Push(action.Target); // Push new state
                    tokens.Dequeue();
                    _options.Out?.WriteLine($"S: {currentToken.Value}");
                    break;

                case ActionType.Reduce:
                    var production = _productions[action.Target.Id];
                    var rawValueStackItems = new Stack<IWithPosition>();

                    // Pop states and values
                    for (var i = 0; i < production.Right.Count; i++)
                    {
                        rawValueStackItems.Push(valueStack.Pop());
                        stateStack.Pop();
                    }

                    // Extract just the values (every other item, starting at 1)
                    var reducibleValues = new List<TAstNode>();

                    // implicit reverse relatively to push sequence
                    foreach (var rawValueStackItem in rawValueStackItems)
                    {
                        var astNode = rawValueStackItem switch
                        {
                            TAstNode node => node,
                            Token token => _options.TokenToNode(token),
                            _ => throw new Exception($"Unexpected raw value stack item: {rawValueStackItem}")
                        };

                        reducibleValues.Add(astNode);
                    }

                    // Apply "reduce" action
                    // Default to first child if no action specified
                    var reduceAction = production.ReduceAction ?? (x => x[0]);
                    var reducedValue = reduceAction(reducibleValues.ToArray())!;

                    reducedValue.WithPosition(reducibleValues);

                    // Get the previous state after popping all the reduced items
                    var prevState = stateStack.Peek();

                    // Push the reduced value and new state
                    valueStack.Push(reducedValue);
                    stateStack.Push(_gotoTable[(prevState, production.Left)]);

                    _options.Out?.WriteLine($"R: {production} [{_gotoTable[(prevState, production.Left)]}]");
                    break;

                case ActionType.Accept:
                    stateStack.Pop();
                    _options.Out?.WriteLine($"A: {valueStack.Peek()}");
                    return (TAstNode)valueStack.Pop();

                case ActionType.Error:
                    throw new Exception($"Syntax error at state {currentState}, token {currentToken.Symbol}, {currentToken.Position}");

                default:
                    throw new InvalidOperationException("Unknown action type");
            }
        }

        throw new Exception("Parsing ended without accept action");
    }
    
    private class LazyEnumerable<T> : IDisposable
    {
        private T _peek;
        private bool _finalWasReturned;
        private readonly T _final;
        private readonly IEnumerator<T> _enumerator;

        public LazyEnumerable(IEnumerable<T> items, T final)
        {
            _final = final;
            _enumerator = items.GetEnumerator();
            _peek = _enumerator.MoveNext() ? _enumerator.Current : _final;
        }

        public bool NotEmpty() => !ReferenceEquals(_peek, _final) || _finalWasReturned == false;

        public T Peek()
        {
            _finalWasReturned = ReferenceEquals(_peek, _final);
            return _peek;
        }

        public void Dequeue()
        {
            _peek = _enumerator.MoveNext() ? _enumerator.Current : _final;
        }

        public void Dispose() => _enumerator.Dispose();
    }

    public class ParserOptions(Func<Token, TAstNode> tokenToNode)
    {
        public TextWriter? Out { get; set; }

        public Func<Token, TAstNode> TokenToNode { get; } = tokenToNode;
    }
}
