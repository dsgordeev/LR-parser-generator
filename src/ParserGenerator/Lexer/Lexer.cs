using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ParserGenerator.Grammar;
using ParserGenerator.Positions;

namespace ParserGenerator.Lexer;

public abstract class Lexer<TLexer> where TLexer : Lexer<TLexer>
{
    protected string _input = string.Empty;
    protected int _position;
    protected int _row = 1;
    protected int _col = 1;

    public TLexer Reset(string input)
    {
        _input = input;
        _position = 0;
        _row = 1;
        _col = 1;

        return (TLexer)this;
    }

    public IEnumerable<Token> Tokenize()
    {
        while (_position < _input.Length)
        {
            var current = Peek();

            if (char.IsWhiteSpace(current))
            {
                HandleWhitespace(current);
                continue;
            }

            // Handle single-line comments
            if (current == '#')
            {
                SkipSingleLineComment();
                continue;
            }

            if (current == '/' && _position + 1 < _input.Length && _input[_position + 1] == '/')
            {
                SkipSingleLineComment();
                continue;
            }

            // Handle multi-line comments
            if (current == '/' && _position + 1 < _input.Length && _input[_position + 1] == '*')
            {
                SkipMultiLineComment();
                continue;
            }

            if (char.IsDigit(current))
            {
                yield return ReadNumber();
                continue;
            }

            if (char.IsLetter(current) || current == '_')
            {
                yield return ReadIdentifierOrKeyword();
                continue;
            }

            if (current == '"')
            {
                yield return ReadString();
                continue;
            }

            if (current == '\'')
            {
                yield return ReadChar();
                continue;
            }

            yield return ReturnOperatorOrPunctuation();
        }
    }

    private void HandleWhitespace(char current)
    {
        if (current == '\n')
        {
            _row++;
            _col = 1;
        }
        else
        {
            _col++;
        }
        Consume();
    }

    private void SkipSingleLineComment()
    {
        // Skip the '#' character
        AdvancePosition(Consume());

        // Consume everything until end of line or file
        while (_position < _input.Length && Peek() != '\n')
        {
            AdvancePosition(Consume());
        }
    }

    private void SkipMultiLineComment()
    {
        var startRow = _row;
        var startCol = _col;

        // Skip the opening '/*'
        AdvancePosition(Consume()); // '/'
        AdvancePosition(Consume()); // '*'

        bool commentClosed = false;

        while (_position < _input.Length)
        {
            var current = Consume();
            AdvancePosition(current);

            if (current == '*' && _position < _input.Length && Peek() == '/')
            {
                // Found closing '*/'
                AdvancePosition(Consume()); // '/'
                commentClosed = true;
                break;
            }

            // Keep track of line numbers for error reporting
            if (current == '\n')
            {
                _row++;
                _col = 1;
            }
        }

        if (!commentClosed)
        {
            throw new Exception($"Unterminated multi-line comment starting at {startRow}:{startCol}");
        }
    }

    private Token ReadNumber()
    {
        var startRow = _row;
        var startCol = _col;
        var sb = new StringBuilder();
        bool hasDecimal = false;
        bool isExponential = false;
        int numberBase = 10; // Default base
        bool isHex = false;

        // Check for hex prefix (0x or 0X)
        if (Peek() == '0' && _position + 1 < _input.Length &&
            (_input[_position + 1] == 'x' || _input[_position + 1] == 'X'))
        {
            isHex = true;
            sb.Append(Consume()); // 0
            AdvancePosition('0');
            sb.Append(Consume()); // x/X
            AdvancePosition('x');
            numberBase = 16;
        }

        while (_position < _input.Length)
        {
            var current = Peek();

            // Handle decimal point for floating-point numbers
            if (current == '.' && !hasDecimal && !isExponential && !isHex)
            {
                // Check if next character is digit (making this a decimal point)
                if (_position + 1 < _input.Length && char.IsDigit(_input[_position + 1]))
                {
                    hasDecimal = true;
                    sb.Append(current);
                    AdvancePosition(current);
                    Consume();
                    continue;
                }
            }

            // Handle exponential notation
            if (current is 'e' or 'E' && !isExponential && !isHex)
            {
                isExponential = true;
                sb.Append(current);
                AdvancePosition(current);
                Consume();

                // Optional + or - after e/E
                if (_position < _input.Length && (Peek() == '+' || Peek() == '-'))
                {
                    var sign = Peek();
                    sb.Append(sign);
                    AdvancePosition(sign);
                    Consume();
                }
                continue;
            }

            // Handle base specification (e.g., "101#2" for binary)
            if (current == '#' && !isHex)
            {
                // Skip the #
                sb.Append(Consume());
                AdvancePosition(current);

                // Read the base
                var baseStr = new StringBuilder();
                while (_position < _input.Length && char.IsDigit(Peek()))
                {
                    var digit = Peek();
                    baseStr.Append(digit);
                    AdvancePosition(digit);
                    Consume();
                }

                if (baseStr.Length == 0)
                {
                    throw new Exception($"Invalid base specification at {_row}:{_col}");
                }

                numberBase = int.Parse(baseStr.ToString());
                if (numberBase is < 2 or > 36)
                {
                    throw new Exception($"Base must be between 2 and 36 at {_row}:{_col}");
                }

                // We've processed the base specifier, break out of the loop
                break;
            }

            // Check for valid digits in the current base
            if (IsValidDigit(current, numberBase))
            {
                sb.Append(current);
                AdvancePosition(current);
                Consume();
            }
            else
            {
                break;
            }
        }

        var valueStr = sb.ToString();
        object numValue;

        try
        {
            if (isHex)
            {
                numValue = Convert.ToInt64(valueStr, 16);
            }
            else if (numberBase != 10)
            {
                if (numberBase is 2 or 8 or 10 or 16)
                {
                    numValue = Convert.ToInt64(valueStr.TrimEnd('#'), numberBase);
                }
                else
                {
                    long numValue1 = 0;
                    long positionBase = 1;
                    for (var i = valueStr.TrimEnd('#').Length - 1; i >= 0; i--)
                    {
                        if (!char.IsDigit(valueStr[i]))
                        {
                            throw new ArgumentOutOfRangeException(nameof(valueStr), "Unexpected digits");
                        }

                        numValue1 += (valueStr[i] - '0') * positionBase;

                        positionBase *= numberBase;
                    }

                    numValue = numValue1;
                }
            }
            else if (hasDecimal || isExponential)
            {
                numValue = double.Parse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            else
            {
                numValue = long.Parse(valueStr);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Invalid number format at {startRow}:{startCol}: {ex.Message}");
        }
        
        return ReturnNumToken(numValue, new Position(startRow, startCol, _row, _col));
    }
    
    private static bool IsValidDigit(char c, int numberBase)
    {
        return numberBase <= 10
            ? char.IsDigit(c) && c - '0' < numberBase
            : char.IsDigit(c) || (char.ToLower(c) >= 'a' && char.ToLower(c) < 'a' + numberBase - 10);
    }

    private Token ReadIdentifierOrKeyword()
    {
        var startRow = _row;
        var startCol = _col;
        var sb = new StringBuilder();

        while (_position < _input.Length)
        {
            var current = Peek();
            if (char.IsLetterOrDigit(current) || current == '_')
            {
                sb.Append(current);
                AdvancePosition(current);
            }
            else 
            {
                break;
            }
            Consume();
        }

        var text = sb.ToString();

        if (KeywordSymbols.TryGetValue(text, out var symbol))
        {
            return ReturnKeywordToken(symbol, text, new Position(startRow, startCol, _row, _col));
        }

        return ReturnIdToken(text, new Position(startRow, startCol, _row, _col));
    }

    private Token ReadString()
    {
        var startRow = _row;
        var startCol = _col;
        AdvancePosition(Consume()); // Skip opening quote
        var sb = new StringBuilder();

        while (_position < _input.Length && Peek() != '"')
        {
            var current = Consume();
            sb.Append(current);
            AdvancePosition(current);
        }

        if (_position >= _input.Length)
        {
            throw new Exception($"Unterminated string literal at {startRow}:{startCol}");
        }

        AdvancePosition(Consume()); // Skip closing quote

        return ReturnStringToken(sb.ToString(), new Position(startRow, startCol, _row, _col));
    }

    private Token ReadChar()
    {
        var startRow = _row;
        var startCol = _col;
        AdvancePosition(Consume()); // Skip opening quote
        var sb = new StringBuilder();

        while (_position < _input.Length && Peek() != '\'')
        {
            var current = Consume();
            sb.Append(current);
            AdvancePosition(current);
        }

        if (_position >= _input.Length)
        {
            throw new Exception($"Unterminated char literal at {startRow}:{startCol}");
        }

        AdvancePosition(Consume()); // Skip closing quote

        return ReturnCharToken(Convert.ToChar(sb.ToString()), new Position(startRow, startCol, _row, _col));
    }

    protected char Peek() => _input[_position];

    protected char Consume() => _input[_position++];

    protected void AdvancePosition(char c)
    {
        if (c == '\n')
        {
            _row++;
            _col = 1;
        }
        else
        {
            _col++;
        }
    }

    protected abstract IReadOnlyDictionary<string, Symbol> KeywordSymbols { get; }

    protected abstract Token ReturnNumToken(object numValue, Position position);

    protected abstract Token ReturnIdToken(string text, Position position);

    protected abstract Token ReturnKeywordToken(Symbol symbol, string text, Position position);

    protected abstract Token ReturnStringToken(string text, Position position);

    protected abstract Token ReturnCharToken(char c, Position position);

    protected abstract Token ReturnOperatorOrPunctuation();
}