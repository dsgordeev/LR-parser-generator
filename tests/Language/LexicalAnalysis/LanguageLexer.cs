using Language.Grammar;
using ParserGenerator.Grammar;
using ParserGenerator.Lexer;
using ParserGenerator.Positions;
using System;
using System.Collections.Generic;

namespace Language.LexicalAnalysis;

public sealed class LanguageLexer : Lexer<LanguageLexer>
{
    protected override IReadOnlyDictionary<string, Symbol> KeywordSymbols => LanguageGrammar.KeywordSymbols;

    protected override Token ReturnNumToken(object numValue, Position position)
    {
        return new Token(LanguageGrammar.NUM, numValue, position);
    }

    protected override Token ReturnIdToken(string text, Position position)
    {
        return new Token(LanguageGrammar.ID, text, position);
    }

    protected override Token ReturnKeywordToken(Symbol symbol, string text, Position position)
    {
        return new Token(symbol, text, position);
    }

    protected override Token ReturnStringToken(string text, Position position)
    {
        return new Token(LanguageGrammar.STRING, text, position);
    }

    protected override Token ReturnCharToken(char c, Position position)
    {
        return new Token(LanguageGrammar.CHAR, c, position);
    }

    protected override Token ReturnOperatorOrPunctuation()
    {
        var startRow = _row;
        var startCol = _col;
        var current = Consume();
        var next = _position < _input.Length ? Peek() : '\0';

        // Handle multi-character operators
        switch (current)
        {
            case '!' when next == '=':
                AdvancePosition(current);
                AdvancePosition(Consume());
                return new Token(LanguageGrammar.NEQ, "!=",
                    new Position(startRow, startCol, _row, _col));
            case '<' when next == '=':
                AdvancePosition(current);
                AdvancePosition(Consume());
                return new Token(LanguageGrammar.LTE, "<=",
                    new Position(startRow, startCol, _row, _col));
            case '>' when next == '=':
                AdvancePosition(current);
                AdvancePosition(Consume());
                return new Token(LanguageGrammar.GTE, ">=",
                    new Position(startRow, startCol, _row, _col));
            case '|' when next == '|':
                AdvancePosition(current);
                AdvancePosition(Consume());
                return new Token(LanguageGrammar.CONCAT, "||",
                    new Position(startRow, startCol, _row, _col));
            case '*' when next == '*':
                AdvancePosition(current);
                AdvancePosition(Consume());
                return new Token(LanguageGrammar.POWER, "**",
                    new Position(startRow, startCol, _row, _col));
        }

        // Handle single-character operators and punctuation
        AdvancePosition(current);

        Token result = current switch
        {
            '+' => new Token(LanguageGrammar.PLUS, "+",
                new Position(startRow, startCol, _row, _col)),
            '-' => new Token(LanguageGrammar.MINUS, "-",
                new Position(startRow, startCol, _row, _col)),
            '*' => new Token(LanguageGrammar.MUL, "*",
                new Position(startRow, startCol, _row, _col)),
            '/' => new Token(LanguageGrammar.DIV, "/",
                new Position(startRow, startCol, _row, _col)),
            '%' => new Token(LanguageGrammar.MOD, "%",
                new Position(startRow, startCol, _row, _col)),
            '=' => new Token(LanguageGrammar.EQ, "=",
                new Position(startRow, startCol, _row, _col)),
            '<' => new Token(LanguageGrammar.LT, "<",
                new Position(startRow, startCol, _row, _col)),
            '>' => new Token(LanguageGrammar.GT, ">",
                new Position(startRow, startCol, _row, _col)),
            '!' => new Token(LanguageGrammar.NOT, "!",
                new Position(startRow, startCol, _row, _col)),
            '&' => new Token(LanguageGrammar.AND, "&",
                new Position(startRow, startCol, _row, _col)),
            '|' => new Token(LanguageGrammar.OR, "|",
                new Position(startRow, startCol, _row, _col)),
            '^' => new Token(LanguageGrammar.XOR, "^",
                new Position(startRow, startCol, _row, _col)),
            '(' => new Token(LanguageGrammar.LPAREN, "(",
                new Position(startRow, startCol, _row, _col)),
            ')' => new Token(LanguageGrammar.RPAREN, ")",
                new Position(startRow, startCol, _row, _col)),
            '{' => new Token(LanguageGrammar.LBRACE, "{",
                new Position(startRow, startCol, _row, _col)),
            '}' => new Token(LanguageGrammar.RBRACE, "}",
                new Position(startRow, startCol, _row, _col)),
            '[' => new Token(LanguageGrammar.LBRACKET, "[",
                new Position(startRow, startCol, _row, _col)),
            ']' => new Token(LanguageGrammar.RBRACKET, "]",
                new Position(startRow, startCol, _row, _col)),
            ',' => new Token(LanguageGrammar.COMMA, ",",
                new Position(startRow, startCol, _row, _col)),
            ';' => new Token(LanguageGrammar.SEMI, ";",
                new Position(startRow, startCol, _row, _col)),
            ':' => new Token(LanguageGrammar.COLON, ":",
                new Position(startRow, startCol, _row, _col)),
            '.' => new Token(LanguageGrammar.DOT, ".",
                new Position(startRow, startCol, _row, _col)),
            _ => throw new Exception($"Unexpected character: '{current}' at {_row}:{_col}")
        };

        return result;
    }
}