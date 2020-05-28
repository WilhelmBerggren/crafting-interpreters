using System;
using System.Collections.Generic;

namespace crafting_interpreters {
    public class LoxFunction : ILoxCallable {
        private Stmt<Void>.Function declaration;
        private Env closure;
        public LoxFunction(Stmt<Void>.Function declaration, Env closure) {
            this.closure = closure;
            this.declaration = declaration;
        }

        public int Arity()
        {
            return declaration.parameters.Count;
        }

        public object Call(Interpreter interpreter, List<object> args)
        {
            var env = new Env(closure);
            for(var i = 0; i < declaration.parameters.Count; i++) {
                env.Define(declaration.parameters[i].lexeme, args[i]);
            }

            try {
                interpreter.ExecuteBlock(declaration.body, env);
            }
            catch (Return returnValue) {
                return returnValue.value;
            }

            return null;
        }

        public override string ToString() {
            return $"<fn {declaration.name.lexeme}>";
        }
    }
}