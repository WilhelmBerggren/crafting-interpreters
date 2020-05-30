using System;
using System.Collections.Generic;

namespace crafting_interpreters
{
    // void is only a return type in C#, so I brought my own. It does nothing.
    public sealed class Void
    {
        private Void() { }
    }

    public class Interpreter : ExprVisitor<Object>, StmtVisitor<Void>
    {
        public Env globals = new Env();
        private Env env;
        private Dictionary<Expr<object>, int> locals = new Dictionary<Expr<object>, int>();
        public Interpreter()
        {
            this.env = globals;
            globals.Define("clock", new Clock());
        }

        public void Interpret(List<Stmt<Void>> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        private void Execute(Stmt<Void> stmt)
        {
            stmt.Accept(this);
        }

        public void Resolve(Expr<object> expr, int depth)
        {
            locals.Add(expr, depth);
        }

        public void ExecuteBlock(List<Stmt<Void>> statements, Env env)
        {
            Env previous = this.env;
            try
            {
                this.env = env;

                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
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

        private string Stringify(Object obj)
        {
            if (obj == null) return "nil";

            if (typeof(Double).IsInstanceOfType(obj))
            {
                var text = obj.ToString();
                if (text.EndsWith(".0"))
                {
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
            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }

            env.Define(stmt.name.lexeme, value);
            return null;
        }

        public Void VisitWhileStmt(Stmt<Void>.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }

            return null;
        }

        public object VisitAssignExpr(Expr<object>.Assign expr)
        {
            object value = Evaluate(expr.value);

            if (locals.ContainsKey(expr))
            {
                int distance = locals[expr];
                env.AssignAt(distance, expr.name, value);
            }
            else
            {
                globals.Assign(expr.name, value);
            }
            return value;
        }

        public object VisitVariableExpr(Expr<object>.Variable expr)
        {
            return LookUpVariable(expr.name, expr);
        }

        private object LookUpVariable(Token name, Expr<object> expr)
        {
            if (locals.ContainsKey(expr))
            {
                int distance = locals[expr];
                return env.GetAt(distance, name.lexeme);
            }
            else
            {
                return globals.Get(name);
            }
        }

        public Void VisitBlockStmt(Stmt<Void>.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Env(env));
            return null;
        }

        public Void VisitIfStmt(Stmt<Void>.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }

            return null;
        }

        public object VisitLogicalExpr(Expr<object>.Logical expr)
        {
            object left = Evaluate(expr.left);

            if (expr.op.type == TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.right);
        }

        public object VisitCallExpr(Expr<object>.Call expr)
        {
            Object callee = Evaluate(expr.callee);

            var args = new List<object>();
            foreach (Expr<object> arg in expr.arguments)
            {
                args.Add(Evaluate(arg));
            }

            if (!(typeof(ILoxCallable).IsInstanceOfType(callee)))
            {
                throw new RuntimeError(expr.paren, "Can only call functions and classes.");
            }

            ILoxCallable function = (ILoxCallable)callee;
            if (args.Count != function.Arity())
            {
                throw new RuntimeError(expr.paren, $"Expected {function.Arity()} arguments but got {args.Count}.");
            }

            return function.Call(this, args);
        }

        public Void VisitFunctionStmt(Stmt<Void>.Function stmt)
        {
            var function = new LoxFunction(stmt, env, false);
            env.Define(stmt.name.lexeme, function);
            return null;
        }

        public Void VisitReturnStmt(Stmt<Void>.Return stmt)
        {
            object value = null;
            if (stmt.value != null) value = Evaluate(stmt.value);

            throw new Return(value);
        }

        public Void VisitClassStmt(Stmt<Void>.Class stmt)
        {
            env.Define(stmt.name.lexeme, null);

            var methods = new Dictionary<string, LoxFunction>();
            foreach (var method in stmt.methods)
            {
                var function = new LoxFunction(method, env, method.name.lexeme == "init");
                methods.Add(method.name.lexeme, function);
            }

            LoxClass klass = new LoxClass(stmt.name.lexeme, methods);
            env.Assign(stmt.name, klass);
            return null;
        }

        public object VisitGetExpr(Expr<object>.Get expr)
        {
            object obj = Evaluate(expr.obj);
            if (typeof(LoxInstance).IsInstanceOfType(obj))
            {
                return ((LoxInstance)obj).Get(expr.name);
            }

            throw new RuntimeError(expr.name, "Only instances have properties.");
        }

        public object VisitSetExpr(Expr<object>.Set expr)
        {
            object obj = Evaluate(expr.obj);

            if (!typeof(LoxInstance).IsInstanceOfType(obj))
            {
                throw new RuntimeError(expr.name, "Only instances have fields.");
            }

            object value = Evaluate(expr.value);
            ((LoxInstance)obj).Set(expr.name, value);

            return value;
        }

        public object VisitThisExpr(Expr<object>.This expr)
        {
            return LookUpVariable(expr.keyword, expr);
        }
    }
}