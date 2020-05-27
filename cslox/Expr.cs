using System;
using System.Collections.Generic;

namespace crafting_interpreters {

    public interface ExprVisitor<R> {
        R VisitAssignExpr(Expr<R>.Assign expr);
        R VisitBinaryExpr(Expr<R>.Binary expr);
        R VisitGroupingExpr(Expr<R>.Grouping expr);
        R VisitLiteralExpr(Expr<R>.Literal expr);
        R VisitLogicalExpr(Expr<R>.Logical expr);
        R VisitUnaryExpr(Expr<R>.Unary expr);
        R VisitVariableExpr(Expr<R>.Variable expr);
    }

    public abstract class Expr<R> {
        public abstract R Accept(ExprVisitor<R> visitor);

        public class Assign : Expr<R>
        {
           public Assign (Token name, Expr<R> value)
            {
                this.name = name;
                this.value = value;
            }

            public Token name { get; }
            public Expr<R> value { get; }

            public override R Accept(ExprVisitor<R> visitor) {
                return visitor.VisitAssignExpr(this);
            }
        }

        public class Binary : Expr<R>
        {
           public Binary (Expr<R> left, Token op, Expr<R> right)
            {
                this.left = left;
                this.op = op;
                this.right = right;
            }

            public Expr<R> left { get; }
            public Token op { get; }
            public Expr<R> right { get; }

            public override R Accept(ExprVisitor<R> visitor) {
                return visitor.VisitBinaryExpr(this);
            }
        }

        public class Grouping : Expr<R>
        {
           public Grouping (Expr<R> expression)
            {
                this.expression = expression;
            }

            public Expr<R> expression { get; }

            public override R Accept(ExprVisitor<R> visitor) {
                return visitor.VisitGroupingExpr(this);
            }
        }

        public class Literal : Expr<R>
        {
           public Literal (Object value)
            {
                this.value = value;
            }

            public Object value { get; }

            public override R Accept(ExprVisitor<R> visitor) {
                return visitor.VisitLiteralExpr(this);
            }
        }

        public class Logical : Expr<R>
        {
           public Logical (Expr<R> left, Token op, Expr<R> right)
            {
                this.left = left;
                this.op = op;
                this.right = right;
            }

            public Expr<R> left { get; }
            public Token op { get; }
            public Expr<R> right { get; }

            public override R Accept(ExprVisitor<R> visitor) {
                return visitor.VisitLogicalExpr(this);
            }
        }

        public class Unary : Expr<R>
        {
           public Unary (Token op, Expr<R> right)
            {
                this.op = op;
                this.right = right;
            }

            public Token op { get; }
            public Expr<R> right { get; }

            public override R Accept(ExprVisitor<R> visitor) {
                return visitor.VisitUnaryExpr(this);
            }
        }

        public class Variable : Expr<R>
        {
           public Variable (Token name)
            {
                this.name = name;
            }

            public Token name { get; }

            public override R Accept(ExprVisitor<R> visitor) {
                return visitor.VisitVariableExpr(this);
            }
        }
    }
}
