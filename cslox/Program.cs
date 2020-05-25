using System;
using System.Collections.Generic;
using System.IO;

namespace crafting_interpreters
{
    public class Program
    {
        private static bool hadError;
        static void Main(string[] args)
        {
            // var expr = new Expr<string>.Binary(
            //     new Expr<string>.Unary(
            //         new Token(TokenType.MINUS, "-", null, 1),
            //         new Expr<string>.Literal(123)
            //     ),
            //     new Token(TokenType.STAR, "*", null, 1),
            //     new Expr<string>.Grouping(
            //         new Expr<string>.Literal(45.67)
            //     )
            // );

            // Console.WriteLine(new AstPrinter().Print(expr));

            if (args.Length > 1)
            {
                Console.WriteLine("Usage: cslox [script]");
                Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunText(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }


        public static void RunText(string source) {
            Run(source);
            if(hadError) Environment.Exit(65);
        }
        public static void RunFile(string path)
        {
            string text = File.ReadAllText(path);
        }

        public static void RunPrompt()
        {
            string input = "";
            while (input != "exit")
            {
                Console.Write("> ");
                input = Console.ReadLine();
                Run(input);
                if (hadError) Environment.Exit(65);
            }
        }

        public static void Run(string source)
        {
            var scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
            hadError = true;
        }
    }
}
