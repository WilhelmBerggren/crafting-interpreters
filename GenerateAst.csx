#!/usr/bin/env dotnet-script
using System;

// --- Representing Code: Challenge 1 ---
// expr -> expr ( "(" ( expr ) "," expr )* )? ")" | "." IDENTIFIER )*
//      | IDENTIFIER
//      | NUMBER

var types = new[] {
    "Binary   : Expr<R> left, Token op, Expr<R> right",
    "Grouping : Expr<R> expression",
    "Literal  : Object value",
    "Unary    : Token op, Expr<R> right"
};

static void DefineVisitor(string baseName, string[] types) {
    WriteLine("    public interface Visitor<R> {");
    foreach(var type in types) {
        string typeName = type.Split(":")[0].Trim();
        WriteLine($"        R visit{typeName}{baseName}({baseName}<R>.{typeName} {baseName.ToLower()});");
    }
    WriteLine("    }");
}

static void DefineType(string baseName, string className, string fieldList) {
    WriteLine();
    WriteLine($"        public class {className} : {baseName}<R>");
    WriteLine("        {");
    WriteLine($"           public {className} ({fieldList})");
    WriteLine("            {");
    var fields = fieldList.Split(", ");
    foreach(var field in fields) {
        var name = field.Split(" ")[1];
        WriteLine($"                this.{name} = {name};");
    }
    WriteLine("            }");
    WriteLine();
    foreach(string field in fields) {
        var type = field.Split(" ")[0];
        var name = field.Split(" ")[1];
        WriteLine($"            public {type} {name} {{ get; }}");
    }

    // Visitor pattern
    WriteLine();
    WriteLine("            public override R Accept(Visitor<R> visitor) {");
    WriteLine($"                return visitor.visit{className}{baseName}(this);");
    WriteLine("            }");
    WriteLine("        }");
    WriteLine();
}

static void DefineAst(string baseName, string[] types) {
    WriteLine("using System;");
    WriteLine();
    WriteLine("namespace crafting_interpreters {");
    WriteLine();
    DefineVisitor(baseName, types);
    WriteLine();
    WriteLine("    public abstract class Expr<R> {");
    WriteLine("        public abstract R Accept(Visitor<R> visitor);");
    foreach(string type in types) {
        string className = type.Split(":")[0].Trim();
        string fields = type.Split(":")[1].Trim();
        DefineType(baseName, className, fields);
    }
    WriteLine();
    WriteLine("    }");
    WriteLine("}");
}

DefineAst("Expr", types);