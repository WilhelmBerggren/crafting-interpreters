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

        private Stmt<Void> Declaration()
        {
            try
            {
                if (Match(TokenType.CLASS)) return ClassDeclaration();
                if (Match(TokenType.FUN)) return Function("function");
                if (Match(TokenType.VAR))
                {
                    return VarDeclaration();
                }

                return Statement();
            }
            catch (ParseError error)
            {
                var e = error;
                Synchronize();
                return null;
            }
        }

        private Stmt<Void> VarDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr<object> initializer = null;
            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration");
            return new Stmt<Void>.Var(name, initializer);
        }

        private Stmt<Void> WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            Expr<object> condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
            Stmt<Void> body = Statement();

            return new Stmt<Void>.While(condition, body);
        }

        private Stmt<Void> Statement()
        {
            if (Match(TokenType.FOR)) return ForStatement();
            if (Match(TokenType.IF)) return IfStatement();
            if (Match(TokenType.PRINT)) return PrintStatement();
            if (Match(TokenType.RETURN)) return ReturnStatement();
            if (Match(TokenType.WHILE)) return WhileStatement();
            if (Match(TokenType.LEFT_BRACE)) return new Stmt<Void>.Block(Block());

            return ExpressionStatement();
        }

        private Stmt<Void> ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt<Void> initializer;
            if (Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(TokenType.VAR))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr<object> condition = null;
            if (!Check(TokenType.SEMICOLON))
            {
                condition = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expr<object> increment = null;
            if (!Check(TokenType.RIGHT_PAREN))
            {
                increment = Expression();
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            Stmt<Void> body = Statement();

            if (increment != null)
            {
                body = new Stmt<Void>.Block(new List<Stmt<Void>> {
                    body,
                    new Stmt<Void>.Expression(increment)
                });
            }

            if (condition == null) condition = new Expr<object>.Literal(true);
            body = new Stmt<Void>.While(condition, body);

            if (initializer != null)
            {
                body = new Stmt<Void>.Block(new List<Stmt<Void>>() { initializer, body });
            }

            return body;
        }

        private Stmt<Void> IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if.'");
            Expr<object> condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt<Void> thenBranch = Statement();
            Stmt<Void> elseBranch = null;
            if (Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new Stmt<Void>.If(condition, thenBranch, elseBranch);
        }

        private Expr<object> Expression()
        {
            return Assignment();
        }

        private Expr<object> Assignment()
        {
            Expr<object> expr = Or();

            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expr<object> value = Assignment();

                if (typeof(Expr<object>.Variable).IsInstanceOfType(expr))
                {
                    Expr<object>.Variable varExpr = (Expr<object>.Variable)expr;
                    return new Expr<object>.Assign(varExpr.name, value);
                }
                else if (typeof(Expr<object>.Get).IsInstanceOfType(expr))
                {
                    Expr<object>.Get getExpr = (Expr<object>.Get)expr;
                    return new Expr<object>.Set(getExpr.obj, getExpr.name, value);
                }

                Error(equals, "Invalid assignment target.");
            }
            return expr;
        }

        private Expr<object> Or()
        {
            Expr<object> expr = And();

            while (Match(TokenType.OR))
            {
                Token op = Previous();
                Expr<object> right = And();
                expr = new Expr<object>.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr<object> And()
        {
            Expr<object> expr = Equality();

            while (Match(TokenType.AND))
            {
                Token op = Previous();
                Expr<object> right = Equality();
                expr = new Expr<object>.Logical(expr, op, right);
            }

            return expr;
        }

        private Stmt<Void> PrintStatement()
        {
            Expr<object> value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Stmt<Void>.Print(value);
        }

        private Stmt<Void> ReturnStatement()
        {
            Token keyword = Previous();
            Expr<object> value = null;
            if (!Check(TokenType.SEMICOLON))
            {
                value = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after return value.");
            return new Stmt<Void>.Return(keyword, value);
        }

        private Stmt<Void> ExpressionStatement()
        {
            Expr<object> expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Stmt<Void>.Expression(expr);
        }

        private Stmt<Void>.Function Function(string kind)
        {
            Token name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");
            Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");
            var parameters = new List<Token>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        Error(Peek(), "Cannot have more than 255 parameters");
                    }

                    parameters.Add(Consume(TokenType.IDENTIFIER, "Expected parameter name."));
                }
                while (Match(TokenType.COMMA));
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

            Consume(TokenType.LEFT_BRACE, $"Expect '{{' before {kind} body.");
            var body = Block();
            return new Stmt<Void>.Function(name, parameters, body);
        }

        private Stmt<Void>.Class ClassDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect class name.");
            Expr<object>.Variable superclass = null;
            if(Match(TokenType.LESS)) {
                Consume(TokenType.IDENTIFIER, "Expect superclass name.");
                superclass = new Expr<object>.Variable(Previous());
            }

            Consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");

            var methods = new List<Stmt<Void>.Function>();
            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                methods.Add(Function("method"));
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");

            return new Stmt<Void>.Class(name, superclass, methods);
        }

        private List<Stmt<Void>> Block()
        {
            var statements = new List<Stmt<Void>>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
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

        private bool Match(TokenType type)
        {
            return Match(new List<TokenType>() { type });
        }

        private bool Match(TokenType type1, TokenType type2)
        {
            return Match(new List<TokenType>() { type1, type2 });
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

            while (Match(TokenType.MINUS, TokenType.PLUS))
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

            while (Match(TokenType.SLASH, TokenType.STAR))
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

            return Call();
        }

        private Expr<object> FinishCall(Expr<object> callee)
        {
            var args = new List<Expr<object>>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (args.Count >= 255)
                    {
                        Error(Peek(), "Cannot have more than 255 arguments.");
                    }
                    args.Add(Expression());
                }
                while (Match(TokenType.COMMA));
            }

            Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

            return new Expr<object>.Call(callee, paren, args);
        }

        private Expr<object> Call()
        {
            Expr<object> expr = Primary();

            while (true)
            {
                if (Match(TokenType.LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else if (Match(TokenType.DOT))
                {
                    Token name = Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                    expr = new Expr<object>.Get(expr, name);
                }
                else
                {
                    break;
                }
            }

            return expr;
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

            if(Match(TokenType.SUPER)) {
                Token keyword = Previous();
                Consume(TokenType.DOT, "Expect '.' after 'super'.");
                Token method = Consume(TokenType.IDENTIFIER, "Expect superclass method name.");
                return new Expr<object>.Super(keyword, method);
            }

            if (Match(TokenType.THIS))
            {
                return new Expr<object>.This(Previous());
            }

            if (Match(TokenType.IDENTIFIER))
            {
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