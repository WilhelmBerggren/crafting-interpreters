using System;
using System.Collections.Generic;
using System.Linq;

namespace crafting_interpreters
{
    public class Resolver : ExprVisitor<object>, StmtVisitor<Void>
    {
        private Interpreter interpreter;
        private Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.NONE;

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        private enum FunctionType
        {
            NONE,
            FUNCTION,
            INITIALIZER,
            METHOD
        }

        private enum ClassType
        {
            NONE,
            CLASS,
            SUBCLASS
        }

        private ClassType currentClass = ClassType.NONE;

        public void Resolve(List<Stmt<Void>> statements)
        {
            foreach (var stmt in statements)
            {
                Resolve(stmt);
            }
        }

        public void Resolve(Stmt<Void> stmt)
        {
            stmt.Accept(this);
        }

        public void Resolve(Expr<object> expr)
        {
            expr.Accept(this);
        }

        private void ResolveFunction(Stmt<Void>.Function function, FunctionType type)
        {
            FunctionType enclosingType = currentFunction;
            currentFunction = type;

            BeginScope();
            foreach (Token param in function.parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.body);
            EndScope();
            currentFunction = enclosingType;
        }

        private void BeginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            scopes.Pop();
        }

        private void Declare(Token name)
        {
            if (scopes.Count == 0) return;

            var scope = scopes.Peek();
            if (scope.ContainsKey(name.lexeme))
            {
                Lox.Error(name, "Variable with this name already declared in this scope.");
            }

            scope.Add(name.lexeme, false);
        }

        private void Define(Token name)
        {
            if (scopes.Count == 0) return;
            scopes.Peek()[name.lexeme] = true;
        }

        private void ResolveLocal(Expr<object> expr, Token name)
        {
            // Took way too long too figure out that C# stack indexes work the opposite way of Java.
            for (int i = 0; i < scopes.Count; i++)
            {
                if (scopes.ElementAt(i).ContainsKey(name.lexeme))
                {
                    interpreter.Resolve(expr, i);
                    return;
                }
            }

        }

        public Void VisitBlockStmt(Stmt<Void>.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return null;
        }

        public Void VisitVarStmt(Stmt<Void>.Var stmt)
        {
            Declare(stmt.name);
            if (stmt.initializer != null)
            {
                Resolve(stmt.initializer);
            }
            Define(stmt.name);
            return null;
        }

        public object VisitVariableExpr(Expr<object>.Variable expr)
        {
            if (scopes.Count != 0)
            {
                bool initialized;
                var exists = scopes.Peek().TryGetValue(expr.name.lexeme, out initialized);
                if (exists && initialized == false)
                {
                    Lox.Error(expr.name, "Cannot read local variable in its own initializer.");
                }
            }

            ResolveLocal(expr, expr.name);
            return null;
        }

        public object VisitAssignExpr(Expr<object>.Assign expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return null;
        }

        public Void VisitFunctionStmt(Stmt<Void>.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public Void VisitExpressionStmt(Stmt<Void>.Expression stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public Void VisitIfStmt(Stmt<Void>.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) Resolve(stmt.elseBranch);
            return null;
        }

        public Void VisitPrintStmt(Stmt<Void>.Print stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public Void VisitReturnStmt(Stmt<Void>.Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                Lox.Error(stmt.keyword, "Cannot return from top-level code.");
            }

            if (currentFunction == FunctionType.INITIALIZER)
            {
                Lox.Error(stmt.keyword, "Cannot return value from an initializer.");
            }

            if (stmt.value != null)
            {
                Resolve(stmt.value);
            }

            return null;
        }

        public Void VisitWhileStmt(Stmt<Void>.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null;
        }

        public object VisitBinaryExpr(Expr<object>.Binary expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitCallExpr(Expr<object>.Call expr)
        {
            Resolve(expr.callee);
            foreach (var argument in expr.arguments)
            {
                Resolve(argument);
            }

            return null;
        }

        public object VisitGroupingExpr(Expr<object>.Grouping expr)
        {
            Resolve(expr.expression);
            return null;
        }

        public object VisitLiteralExpr(Expr<object>.Literal expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Expr<object>.Logical expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitUnaryExpr(Expr<object>.Unary expr)
        {
            Resolve(expr.right);
            return null;
        }

        public Void VisitClassStmt(Stmt<Void>.Class stmt)
        {
            var enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            Declare(stmt.name);
            Define(stmt.name);

            if(stmt.superclass != null && stmt.name.lexeme == stmt.superclass.name.lexeme) {
                Lox.Error(stmt.superclass.name, "A class cannot inherit from itself.");
            }

            if(stmt.superclass != null) {
                BeginScope();
                scopes.Peek().Add("super", true);
            }

            BeginScope();
            scopes.Peek().Add("this", true);

            foreach (var method in stmt.methods)
            {
                var declaration = FunctionType.METHOD;
                if (method.name.lexeme == "init")
                {
                    declaration = FunctionType.INITIALIZER;
                }
                ResolveFunction(method, declaration);
            }
            EndScope();

            if(stmt.superclass != null) {
                EndScope();
            }

            currentClass = enclosingClass;
            return null;
        }

        public object VisitGetExpr(Expr<object>.Get expr)
        {
            Resolve(expr.obj);
            return null;
        }

        public object VisitSetExpr(Expr<object>.Set expr)
        {
            Resolve(expr.value);
            Resolve(expr.obj);
            return null;
        }

        public object VisitThisExpr(Expr<object>.This expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Lox.Error(expr.keyword, "Cannot use 'this' outside of class");
                return null;
            }
            ResolveLocal(expr, expr.keyword);
            return null;
        }

        public object VisitSuperExpr(Expr<object>.Super expr)
        {
            if(currentClass == ClassType.NONE) {
                Lox.Error(expr.keyword, "Cannot use 'super' outside of a class.");
            } else if(currentClass != ClassType.SUBCLASS) {
                Lox.Error(expr.keyword, "Cannot use 'super' in a class with no superclass.");
            }
            
            ResolveLocal(expr, expr.keyword);
            return null;
        }
    }
}