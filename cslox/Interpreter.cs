using System;
using System.Collections.Generic;

namespace crafting_interpreters
{
    // void is only a return type in C#, so I brought my own. It does nothing.
    public sealed class Void {
        private Void() { }
    }

    public class Interpreter : ExprVisitor<Object>, StmtVisitor<Void>
    {
        private Env env = new Env();
        public void Interpret(List<Stmt<Void>> statements) {
            try {
                foreach(var statement in statements) {
                    Execute(statement);
                }
            }
            catch(RuntimeError error) {
                Lox.RuntimeError(error);
            }
        }

        private void Execute(Stmt<Void> stmt) {
            stmt.Accept(this);
        }

        private void ExecuteBlock(List<Stmt<Void>> statements, Env environment) {
            Env previous = this.env;
            try {
                this.env = environment;

                foreach(var statement in statements) {
                    Execute(statement);
                }
            }
            finally {
                this.env = previous;
            }
        }

        private Object Evaluate(Expr<Object> expr)
        {
            return expr.Accept(this);
        }

        public object VisitLiteralExpr(Expr<object>.Literal expr)
        {
            return expr.value;
        }

        public object VisitGroupingExpr(Expr<object>.Grouping expr)
        {
            return expr.expression;
        }

        public object VisitUnaryExpr(Expr<object>.Unary expr)
        {
            Object right = Evaluate(expr.right);

            switch (expr.op.type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                CheckNumberOperand(expr.op, right);
                    return -(double)right;
            }

            return null;
        }

        private bool IsTruthy(object obj)
        {
            if (obj == null) return false;
            if (typeof(bool).IsInstanceOfType(obj)) return (bool)obj;
            return true;
        }

        private void CheckNumberOperand(Token op, object operand)
        {
            if (typeof(Double).IsInstanceOfType(operand)) return;
            throw new RuntimeError(op, "Operand must be a number.");
        }

        public object VisitBinaryExpr(Expr<object>.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr.op.type)
            {
                case TokenType.BANG_EQUAL: return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL: return IsEqual(left, right);
                case TokenType.GREATER:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left <= (double)right;
                case TokenType.MINUS:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left - (double)right;
                case TokenType.PLUS:
                    if (typeof(double).IsInstanceOfType(left) && typeof(double).IsInstanceOfType(right))
                    {
                        return (double)left + (double)right;
                    }
                    if (typeof(string).IsInstanceOfType(left) && typeof(string).IsInstanceOfType(right))
                    {
                        return (string)left + (string)right;
                    }

                    throw new RuntimeError(expr.op, "Operands must be two numbers or two string");
                case TokenType.SLASH:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left * (double)right;
            }

            return null;
        }

        private void CheckNumberOperand(Token op, object left, object right)
        {
            if (typeof(Double).IsInstanceOfType(left) && typeof(Double).IsInstanceOfType(right)) return;
            throw new RuntimeError(op, "Operands must be numbers.");
        }

        private bool IsEqual(object left, object right)
        {
            if (left == null && right == null) return true;
            if (left == null) return false;

            return left.Equals(right);
        }

        private string Stringify(Object obj) {
            if(obj == null) return "nil";

            if(typeof(Double).IsInstanceOfType(obj)) {
                var text = obj.ToString();
                if(text.EndsWith(".0")) {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return obj.ToString();
        }

        public Void VisitExpressionStmt(Stmt<Void>.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public Void VisitPrintStmt(Stmt<Void>.Print stmt)
        {
            Object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public Void VisitVarStmt(Stmt<Void>.Var stmt)
        {
            object value = null;
            if(stmt.initializer != null) {
                value = Evaluate(stmt.initializer);
            }

            env.Define(stmt.name.lexeme, value);
            return null;
        }

        public object VisitAssignExpr(Expr<object>.Assign expr) {
            object value = Evaluate(expr.value);

            env.Assign(expr.name, value);
            return value;
        }

        public object VisitVariableExpr(Expr<object>.Variable expr)
        {
            return env.Get(expr.name);
        }

        public Void VisitBlockStmt(Stmt<Void>.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Env(env));
            return null;
        }
    }
}