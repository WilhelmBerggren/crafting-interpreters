using System;

namespace crafting_interpreters {

    public interface Visitor<R> {
        R visitBinaryExpr(Expr<R>.Binary expr);
        R visitGroupingExpr(Expr<R>.Grouping expr);
        R visitLiteralExpr(Expr<R>.Literal expr);
        R visitUnaryExpr(Expr<R>.Unary expr);
    }

    public abstract class Expr<R> {
        public abstract R Accept(Visitor<R> visitor);

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

            public override R Accept(Visitor<R> visitor) {
                return visitor.visitBinaryExpr(this);
            }
        }


        public class Grouping : Expr<R>
        {
           public Grouping (Expr<R> expression)
            {
                this.expression = expression;
            }

            public Expr<R> expression { get; }

            public override R Accept(Visitor<R> visitor) {
                return visitor.visitGroupingExpr(this);
            }
        }


        public class Literal : Expr<R>
        {
           public Literal (Object value)
            {
                this.value = value;
            }

            public Object value { get; }

            public override R Accept(Visitor<R> visitor) {
                return visitor.visitLiteralExpr(this);
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

            public override R Accept(Visitor<R> visitor) {
                return visitor.visitUnaryExpr(this);
            }
        }


    }
}
