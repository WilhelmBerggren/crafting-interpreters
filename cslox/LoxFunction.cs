using System;
using System.Collections.Generic;

namespace crafting_interpreters
{
    public class LoxFunction : ILoxCallable
    {
        private Stmt<Void>.Function declaration;
        private Env closure;
        private readonly bool isInitializer;
        public LoxFunction(Stmt<Void>.Function declaration, Env closure, bool isInitializer)
        {
            this.closure = closure;
            this.declaration = declaration;
            this.isInitializer = isInitializer;
        }

        public LoxFunction Bind(LoxInstance instance)
        {
            Env env = new Env(closure);
            env.Define("this", instance);
            return new LoxFunction(declaration, env, isInitializer);
        }

        public int Arity()
        {
            return declaration.parameters.Count;
        }

        public object Call(Interpreter interpreter, List<object> args)
        {
            var env = new Env(closure);
            for (var i = 0; i < declaration.parameters.Count; i++)
            {
                env.Define(declaration.parameters[i].lexeme, args[i]);
            }

            try
            {
                interpreter.ExecuteBlock(declaration.body, env);
            }
            catch (Return returnValue)
            {
                if (isInitializer) return closure.GetAt(0, "this");

                return returnValue.value;
            }

            if (isInitializer) return closure.GetAt(0, "this");
            return null;
        }

        public override string ToString()
        {
            return $"<fn {declaration.name.lexeme}>";
        }
    }
}