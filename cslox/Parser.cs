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

        public List<Stmt<Void>> Parse()
        {
            List<Stmt<Void>> statements = new List<Stmt<Void>>();
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Stmt<Void> Declaration() {
            try {
                if(Match(TokenType.VAR)) {
                    return VarDeclaration();
                }

                return Statement();
            }
            catch (ParseError error) {
                var e = error;
                Synchronize();
                return null;
            }
        }

        private Stmt<Void> VarDeclaration() {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr<object> initializer = null;
            if(Match(TokenType.EQUAL)) {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration");
            return new Stmt<Void>.Var(name, initializer);
        }

        private Stmt<Void> Statement()
        {
            if (Match(TokenType.PRINT)) return PrintStatement();
            if (Match(TokenType.LEFT_BRACE)) return new Stmt<Void>.Block(Block());

            return ExpressionStatement();
        }

        private Expr<object> Expression()
        {
            return Assignment();
        }

        private Expr<object> Assignment() {
            Expr<object> expr = Equality();

            if(Match(TokenType.EQUAL)) {
                Token equals = Previous();
                Expr<object> value = Assignment();

                if(typeof(Expr<object>.Variable).IsInstanceOfType(expr)) {
                    Expr<object>.Variable varExpr = (Expr<object>.Variable) expr;
                    return new Expr<object>.Assign(varExpr.name, value);
                }

                Error(equals, "Invalid assignment target.");
            }
            return expr;
        }

        private Stmt<Void> PrintStatement()
        {
            Expr<object> value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Stmt<Void>.Print(value);
        }

        private Stmt<Void> ExpressionStatement()
        {
            Expr<object> expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Stmt<Void>.Expression(expr);
        }

        private List<Stmt<Void>> Block() {
            var statements = new List<Stmt<Void>>();

            while(!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Expr<object> Equality()
        {
            Expr<object> expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
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

        private bool Match(TokenType type) {
            return Match(new List<TokenType>() {type});
        }

        private bool Match(TokenType type1, TokenType type2) {
            return Match(new List<TokenType>() {type1, type2});
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

            while (Match(TokenType.MINUS, TokenType.PLUS ))
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

            while (Match(TokenType.SLASH, TokenType.STAR ))
            {
                Token op = Previous();
                Expr<object> right = Unary();
                expr = new Expr<object>.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr<object> Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token op = Previous();
                Expr<object> right = Unary();
                return new Expr<object>.Unary(op, right);
            }

            return Primary();
        }

        private Expr<object> Primary()
        {
            if (Match(TokenType.FALSE)) return new Expr<object>.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr<object>.Literal(true);
            if (Match(TokenType.NIL)) return new Expr<object>.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr<object>.Literal(Previous().literal);
            }

            if (Match(TokenType.IDENTIFIER)) {
                return new Expr<object>.Variable(Previous());
            }

            if (Match(TokenType.LEFT_PAREN))
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