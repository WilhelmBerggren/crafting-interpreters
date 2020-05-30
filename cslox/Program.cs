using System;
using System.Collections.Generic;
using System.IO;

namespace crafting_interpreters
{
    public class Lox
    {
        private static Interpreter interpreter = new Interpreter();
        private static bool hadError = false;
        private static bool hadRuntimeError = false;
        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: cslox [script]");
                Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
                if(hadError) Environment.Exit(65);
                if(hadRuntimeError) Environment.Exit(70);
            }
            else
            {
                Console.WriteLine("Entering REPL");
                RunPrompt();
            }
        }


        public static void RunText(string source)
        {
            Run(source);
            if (hadError) Environment.Exit(65);
        }
        public static void RunFile(string path)
        {
            string source = File.ReadAllText(path);
            Run(source);
            if (hadError) Environment.Exit(65);
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
            Parser parser = new Parser(tokens);
            List<Stmt<Void>> statements = parser.Parse();

            if (hadError) return;

            var resolver = new Resolver(interpreter);
            resolver.Resolve(statements);

            if (hadError) return;

            interpreter.Interpret(statements);
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                Report(token.line, "at end", message);
            }
            else
            {
                Report(token.line, $"at '{token.lexeme}'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.Error.WriteLine(error.Message);
            Console.Error.WriteLine($"line {error.token.line}");
            hadRuntimeError = true;
        }

        public static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
            hadError = true;
        }
    }
}
