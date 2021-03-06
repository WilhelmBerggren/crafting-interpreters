using System;
using System.Collections.Generic;

namespace crafting_interpreters {

    public interface ExprVisitor<R> {
        R VisitAssignExpr(Expr<R>.Assign expr);
        R VisitBinaryExpr(Expr<R>.Binary expr);
        R VisitCallExpr(Expr<R>.Call expr);
        R VisitGetExpr(Expr<R>.Get expr);
        R VisitGroupingExpr(Expr<R>.Grouping expr);
        R VisitLiteralExpr(Expr<R>.Literal expr);
        R VisitLogicalExpr(Expr<R>.Logical expr);
        R VisitSetExpr(Expr<R>.Set expr);
        R VisitSuperExpr(Expr<R>.Super expr);
        R VisitThisExpr(Expr<R>.This expr);
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

        public class Call : Expr<R>
        {
           public Call (Expr<R> callee, Token paren, List<Expr<R>> arguments)
            {
                this.callee = callee;
                this.paren = paren;
                this.arguments = arguments;
            }

            public Expr<R> callee { get; }
            public Token paren { get; }
            public List<Expr<R>> arguments { get; }

            public override R Accept(ExprVisitor<R> visitor) {
                return visitor.VisitCallExpr(this);
            }
        }

        public class Get : Expr<R>
        {
           public Get (Expr<R> obj, Token name)
            {
                this.obj = obj;
                this.name = name;
            }

            public Expr<R> obj { get; }
            public Token name { get; }

            public override R Accept(ExprVisitor<R> visitor) {
                return visitor.VisitGetExpr(this);
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

        public class Set : Expr<R>
        {
           public Set (Expr<R> obj, Token name, Expr<R> value)
            {
                this.obj = obj;
                this.name = name;
                this.value = value;
            }

            public Expr<R> obj { get; }
            public Token name { get; }
            public Expr<R> value { get; }

            public override R Accept(ExprVisitor<R> visitor) {
                return visitor.VisitSetExpr(this);
            }
        }

        public class Super : Expr<R>
        {
           public Super (Token keyword, Token method)
            {
                this.keyword = keyword;
                this.method = method;
            }

            public Token keyword { get; }
            public Token method { get; }

            public override R Accept(ExprVisitor<R> visitor) {
                return visitor.VisitSuperExpr(this);
            }
        }

        public class This : Expr<R>
        {
           public This (Token keyword)
            {
                this.keyword = keyword;
            }

            public Token keyword { get; }

            public override R Accept(ExprVisitor<R> visitor) {
                return visitor.VisitThisExpr(this);
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
