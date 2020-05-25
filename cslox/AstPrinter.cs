namespace crafting_interpreters {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AstPrinter : Visitor<string>
    {
        public string Print(Expr<string> expr) {
            return expr.Accept(this);
        }
        public string visitBinaryExpr(Expr<string>.Binary expr)
        {
            return Parenthesize(expr.op.lexeme, new List<Expr<string>> {expr.left, expr.right});
        }

        public string visitGroupingExpr(Expr<string>.Grouping expr)
        {
            return Parenthesize("group", new List<Expr<string>> {expr.expression});
        }

        public string visitLiteralExpr(Expr<string>.Literal expr)
        {
            if(expr.value == null) return "nil";
            return expr.value.ToString();
        }

        public string visitUnaryExpr(Expr<string>.Unary expr)
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
    }
}
