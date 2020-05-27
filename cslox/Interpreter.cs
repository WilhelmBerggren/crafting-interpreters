using System;

namespace crafting_interpreters
{
    public class Interpreter : Visitor<Object>
    {
        public void Interpret(Expr<Object> expr) {
            try {
                Object value = Evaluate(expr);
                Console.WriteLine(Stringify(value));
            }
            catch(RuntimeError error) {
                Lox.RuntimeError(error);
            }

        }
        private Object Evaluate(Expr<Object> expr)
        {
            return expr.Accept(this);
        }

        public object visitLiteralExpr(Expr<object>.Literal expr)
        {
            return expr.value;
        }

        public object visitGroupingExpr(Expr<object>.Grouping expr)
        {
            return expr.expression;
        }

        public object visitUnaryExpr(Expr<object>.Unary expr)
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

        public object visitBinaryExpr(Expr<object>.Binary expr)
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
    }
}