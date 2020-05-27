using System;
using System.Collections.Generic;

namespace crafting_interpreters {

    public interface StmtVisitor<R> {
        R VisitBlockStmt(Stmt<R>.Block stmt);
        R VisitExpressionStmt(Stmt<R>.Expression stmt);
        R VisitIfStmt(Stmt<R>.If stmt);
        R VisitPrintStmt(Stmt<R>.Print stmt);
        R VisitVarStmt(Stmt<R>.Var stmt);
        R VisitWhileStmt(Stmt<R>.While stmt);
    }

    public abstract class Stmt<R> {
        public abstract R Accept(StmtVisitor<R> visitor);

        public class Block : Stmt<R>
        {
           public Block (List<Stmt<R>> statements)
            {
                this.statements = statements;
            }

            public List<Stmt<R>> statements { get; }

            public override R Accept(StmtVisitor<R> visitor) {
                return visitor.VisitBlockStmt(this);
            }
        }

        public class Expression : Stmt<R>
        {
           public Expression (Expr<object> expression)
            {
                this.expression = expression;
            }

            public Expr<object> expression { get; }

            public override R Accept(StmtVisitor<R> visitor) {
                return visitor.VisitExpressionStmt(this);
            }
        }

        public class If : Stmt<R>
        {
           public If (Expr<object> condition, Stmt<R> thenBranch, Stmt<R> elseBranch)
            {
                this.condition = condition;
                this.thenBranch = thenBranch;
                this.elseBranch = elseBranch;
            }

            public Expr<object> condition { get; }
            public Stmt<R> thenBranch { get; }
            public Stmt<R> elseBranch { get; }

            public override R Accept(StmtVisitor<R> visitor) {
                return visitor.VisitIfStmt(this);
            }
        }

        public class Print : Stmt<R>
        {
           public Print (Expr<object> expression)
            {
                this.expression = expression;
            }

            public Expr<object> expression { get; }

            public override R Accept(StmtVisitor<R> visitor) {
                return visitor.VisitPrintStmt(this);
            }
        }

        public class Var : Stmt<R>
        {
           public Var (Token name, Expr<object> initializer)
            {
                this.name = name;
                this.initializer = initializer;
            }

            public Token name { get; }
            public Expr<object> initializer { get; }

            public override R Accept(StmtVisitor<R> visitor) {
                return visitor.VisitVarStmt(this);
            }
        }

        public class While : Stmt<R>
        {
           public While (Expr<object> condition, Stmt<R> body)
            {
                this.condition = condition;
                this.body = body;
            }

            public Expr<object> condition { get; }
            public Stmt<R> body { get; }

            public override R Accept(StmtVisitor<R> visitor) {
                return visitor.VisitWhileStmt(this);
            }
        }
    }
}
