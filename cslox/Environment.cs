using System;
using System.Collections.Generic;

namespace crafting_interpreters {
    public class Env {
        private readonly Env enclosing;
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public Env() {
            enclosing = null;
        }

        public Env(Env enclosing) {
            this.enclosing = enclosing;
        }

        public void Define(string name, object value) {
            values.Add(name, value);
        }

        public Env Ancestor(int distance) {
            Env env = this;
            for(int i = 0; i < distance; i++) {
                env = env.enclosing;
            }

            return env;
        }

        public object GetAt(int distance, string name) {
            return Ancestor(distance).values[name];
        }

        public void AssignAt(int distance, Token name, object value) {
            Ancestor(distance).values[name.lexeme] = value;
        }

        public object Get(Token name) {
            if(values.ContainsKey(name.lexeme)) {
                return values[name.lexeme];
            }
            if(enclosing != null) {
                return enclosing.Get(name);
            }
            
            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }

        public void Assign(Token name, object value) {
            if(values.ContainsKey(name.lexeme)) {
                values[name.lexeme] = value;
                return;
            }

            if(enclosing != null) {
                enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }
    }
}