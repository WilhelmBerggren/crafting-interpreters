namespace crafting_interpreters {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AstPrinter : ExprVisitor<string>
    {
        public string Print(Expr<string> expr) {
            return expr.Accept(this);
        }
        public string VisitBinaryExpr(Expr<string>.Binary expr)
        {
            return Parenthesize(expr.op.lexeme, new List<Expr<string>> {expr.left, expr.right});
        }

        public string VisitGroupingExpr(Expr<string>.Grouping expr)
        {
            return Parenthesize("group", new List<Expr<string>> {expr.expression});
        }

        public string VisitLiteralExpr(Expr<string>.Literal expr)
        {
            if(expr.value == null) return "nil";
            return expr.value.ToString();
        }

        public string VisitUnaryExpr(Expr<string>.Unary expr)
        {
            return Parenthesize(expr.op.lexeme, new List<Expr<string>> {expr.right});
        }

        public string Parenthesize(string name, IEnumerable<Expr<string>> exprs) {
            var str = exprs.Aggregate(name, (acc, e) => {
                // Console.WriteLine(e.Accept(this).;
                return $"{acc}  {e.Accept(this)}";
            });
            return $"({str})";
        }

        public string VisitVariableExpr(Expr<string>.Variable expr)
        {
            throw new NotImplementedException();
        }

        public string VisitAssignExpr(Expr<string>.Assign expr)
        {
            throw new NotImplementedException();
        }

        public string VisitLogicalExpr(Expr<string>.Logical expr)
        {
            throw new NotImplementedException();
        }

        public string VisitCallExpr(Expr<string>.Call expr)
        {
            throw new NotImplementedException();
        }
    }
}
