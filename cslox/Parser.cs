using System;
using System.Collections.Generic;

namespace crafting_interpreters
{
    public class Parser
    {
        private class ParseError : SystemException { }
        private List<Token> Tokens { get; }
        private int Current { get; set; } = 0;

        public Parser(List<Token> tokens)
        {
            this.Tokens = tokens;
        }

        public Expr<object> Parse()
        {
            try
            {
                return Expression();
            }
            catch (ParseError error)
            {
                var a = error;
                return null;
            }
        }

        private Expr<object> Expression()
        {
            return Equality();
        }

        private Expr<object> Equality()
        {
            Expr<object> expr = Comparison();

            while (Match(new List<TokenType>() { TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL }))
            {
                Token op = Previous();
                Expr<object> right = Comparison();
                expr = new Expr<object>.Binary(expr, op, expr);
            }

            return expr;
        }

        private bool Match(List<TokenType> types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) Current++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().type == TokenType.EOF;
        }

        private Token Peek()
        {
            return Tokens[Current];
        }

        private Token Previous()
        {
            return Tokens[Current - 1];
        }

        private ParseError Error(Token token, string message)
        {
            Lox.Error(token, message);
            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().type == TokenType.SEMICOLON) return;

                switch (Peek().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                Advance();
            }
        }

        private Expr<object> Comparison()
        {
            Expr<object> expr = Addition();

            while (Match(new List<TokenType>() { TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL }))
            {
                Token op = Previous();
                Expr<object> right = Addition();
                expr = new Expr<object>.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr<object> Addition()
        {
            Expr<object> expr = Multiplication();

            while (Match(new List<TokenType>() { TokenType.MINUS, TokenType.PLUS }))
            {
                Token op = Previous();
                Expr<object> right = Multiplication();
                expr = new Expr<object>.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr<object> Multiplication()
        {
            Expr<object> expr = Unary();

            while (Match(new List<TokenType>() { TokenType.SLASH, TokenType.STAR }))
            {
                Token op = Previous();
                Expr<object> right = Unary();
                expr = new Expr<object>.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr<object> Unary()
        {
            if (Match(new List<TokenType>() { TokenType.BANG, TokenType.MINUS }))
            {
                Token op = Previous();
                Expr<object> right = Unary();
                return new Expr<object>.Unary(op, right);
            }

            return Primary();
        }

        private Expr<object> Primary()
        {
            if (Match(new List<TokenType>() { TokenType.FALSE })) return new Expr<object>.Literal(false);
            if (Match(new List<TokenType>() { TokenType.TRUE })) return new Expr<object>.Literal(true);
            if (Match(new List<TokenType>() { TokenType.NIL })) return new Expr<object>.Literal(null);

            if (Match(new List<TokenType>() { TokenType.NUMBER, TokenType.STRING }))
            {
                return new Expr<object>.Literal(Previous().literal);
            }

            if (Match(new List<TokenType>() { TokenType.LEFT_PAREN }))
            {
                Expr<object> expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr<object>.Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }
    }
}