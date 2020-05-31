#!/usr/bin/env dotnet-script
using System;

// --- Representing Code: Challenge 1 ---
// expr -> expr ( "(" ( expr ) "," expr )* )== null ? ")" | "." IDENTIFIER )*
//      | IDENTIFIER
//      | NUMBER

static void DefineVisitor(string baseName, string generic, string[] types)
{
    WriteLine($"    public interface {baseName}Visitor{(generic != null ? $"<{generic}>" : "")} {{");
    foreach (var type in types)
    {
        string typeName = type.Split(":")[0].Trim();
        WriteLine($"        {(generic != null ? $"{generic}" : "void")} Visit{typeName}{baseName}({baseName}{(generic != null ? $"<{generic}>" : "")}.{typeName} {baseName.ToLower()});");
    }
    WriteLine("    }");
}

static void DefineType(string baseName, string generic, string className, string fieldList)
{
    WriteLine();
    WriteLine($"        public class {className} : {baseName}{(generic != null ? $"<{generic}>" : "")}");
    WriteLine("        {");
    WriteLine($"           public {className} ({fieldList})");
    WriteLine("            {");
    var fields = fieldList.Split(", ");
    foreach (var field in fields)
    {
        var name = field.Split(" ")[1];
        WriteLine($"                this.{name} = {name};");
    }
    WriteLine("            }");
    WriteLine();
    foreach (string field in fields)
    {
        var type = field.Split(" ")[0];
        var name = field.Split(" ")[1];
        WriteLine($"            public {type} {name} {{ get; }}");
    }

    // Visitor pattern
    WriteLine();
    WriteLine($"            public override {(generic != null ? $"{generic}" : "void")} Accept({baseName}Visitor{(generic != null ? $"<{generic}>" : "")} visitor) {{");
    WriteLine($"                return visitor.Visit{className}{baseName}(this);");
    WriteLine("            }");
    WriteLine("        }");
}

static void DefineAst(string baseName, string generic, string[] types)
{
    WriteLine("using System;");
    WriteLine("using System.Collections.Generic;");
    WriteLine();
    WriteLine("namespace crafting_interpreters {");
    WriteLine();
    DefineVisitor(baseName, generic, types);
    WriteLine();
    WriteLine($"    public abstract class {baseName}{(generic != null ? $"<{generic}>" : "")} {{");
    WriteLine($"        public abstract {(generic != null ? $"{generic}" : "void")} Accept({baseName}Visitor{(generic != null ? $"<{generic}>" : "")} visitor);");
    foreach (string type in types)
    {
        string className = type.Split(":")[0].Trim();
        string fields = type.Split(":")[1].Trim();
        DefineType(baseName, generic, className, fields);
    }
    WriteLine("    }");
    WriteLine("}");
}

DefineAst("Expr", "R", new[] {
    "Assign   : Token name, Expr<R> value",
    "Binary   : Expr<R> left, Token op, Expr<R> right",
    "Call     : Expr<R> callee, Token paren, List<Expr<R>> arguments",
    "Get      : Expr<R> obj, Token name",
    "Grouping : Expr<R> expression",
    "Literal  : Object value",
    "Logical  : Expr<R> left, Token op, Expr<R> right",
    "Set      : Expr<R> obj, Token name, Expr<R> value",
    "Super    : Token keyword, Token method",
    "This     : Token keyword",
    "Unary    : Token op, Expr<R> right",
    "Variable : Token name"
});

// DefineAst("Stmt", "R", new[] {
//     "Block      : List<Stmt<R>> statements",
//     "Class      : Token name, Expr<object>.Variable superclass, List<Stmt<R>.Function> methods",
//     "Expression : Expr<object> expression",
//     "Function   : Token name, List<Token> parameters, List<Stmt<R>> body",
//     "If         : Expr<object> condition, Stmt<R> thenBranch, Stmt<R> elseBranch",
//     "Print      : Expr<object> expression",
//     "Return     : Token keyword, Expr<object> value",
//     "Var        : Token name, Expr<object> initializer",
//     "While      : Expr<object> condition, Stmt<R> body"
// });