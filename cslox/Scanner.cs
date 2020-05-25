using System;
using System.Collections.Generic;

namespace crafting_interpreters
{
    public class Scanner
    {
        private string Source { get; }

        private List<Token> tokens = new List<Token>();
        private Dictionary<string, TokenType> Keywords { get; } = new Dictionary<string, TokenType>() {
            {"and", TokenType.AND},
            {"class",  TokenType.CLASS},
            {"else",   TokenType.ELSE},
            {"false",  TokenType.FALSE},
            {"for",    TokenType.FOR},
            {"fun",    TokenType.FUN},
            {"if",     TokenType.IF},
            {"nil",    TokenType.NIL},
            {"or",     TokenType.OR},
            {"print",  TokenType.PRINT},
            {"return", TokenType.RETURN},
            {"super",  TokenType.SUPER},
            {"this",   TokenType.THIS},
            {"true",   TokenType.TRUE},
            {"var",    TokenType.VAR},
            {"while",  TokenType.WHILE},
        };
        public int Line { get; set; } = 1;
        private int Start { get; set; } = 0;
        private int Current { get; set; } = 0;

        public Scanner(string source)
        {
            this.Source = source;
        }
        public List<Token> ScanTokens()
        {
            while (!isAtEnd())
            {
                Start = Current;
                ScanToken();
            }
            tokens.Add(new Token(TokenType.EOF, "", null, Line));
            return tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.                
                        while (Peek() != '\n' && !isAtEnd()) Advance();
                    }
                    else if (Match('*')) { 
                        Comment(); 
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.                      
                    break;

                case '\n':
                    Line++;
                    break;
                case '"': String(); break;
                default:
                    if (IsDigit(c)) Number();
                    else if (IsAlpha(c)) Identifier();
                    else Lox.Error(Line, "Unexpected character.");
                    break;
            }
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            string text = Source.Substring(Start, Current - Start);

            TokenType type = Keywords.GetValueOrDefault(text);
            if (type == default) type = TokenType.IDENTIFIER;

            AddToken(type);
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                     c == '_';
        }

        private void Number()
        {
            while (IsDigit(Peek())) Advance();

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();
                while (IsDigit(Peek())) Advance();
            }

            Console.WriteLine(Source);
            Console.WriteLine($"number at: {Start}, {Current}");
            Console.WriteLine(Source.Substring(Start, Current - Start));
            AddToken(TokenType.NUMBER, Double.Parse(Source.Substring(Start, Current - Start)));
        }

        private char PeekNext()
        {
            if (Current + 1 >= Source.Length) return '\0';
            return Source[Current + 1];
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private void String()
        {
            while (Peek() != '"' && !isAtEnd())
            {
                if (Peek() == '\n') Line++;
                Advance();
            }
            if (isAtEnd())
            {
                Lox.Error(Line, "Unterminated string.");
                return;
            }

            Advance();

            string value = Source.Substring(Start + 1, Current - Start - 1);
            AddToken(TokenType.STRING, value);
        }

        private void Comment()
        {
            while (!(Peek() == '*' && PeekNext() == '/') && !isAtEnd())
            {
                if (Peek() == '\n') Line++;

                Advance();

                if(Peek() == '/' && PeekNext() == '*') {
                    Advance();
                    Comment();
                }
            }

            if (isAtEnd())
            {
                Lox.Error(Line, "Unterminated comment.");
                return;
            }

            string value = Source.Substring(Start + 2, Current - Start - 2);

            Advance();
            Advance();
        }

        private char Peek()
        {
            if (isAtEnd()) return '\0';
            return Source[Current];
        }

        private bool Match(char expected)
        {
            if (isAtEnd()) return false;
            if (Source[Current] != expected) return false;

            Current++;
            return true;
        }
        private char Advance()
        {
            Current++;
            return Source[Current - 1];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            string text = Source.Substring(Start, Current - Start);
            tokens.Add(new Token(type, text, literal, Line));
        }

        private bool isAtEnd()
        {
            return Current >= Source.Length;
        }
    }
}
