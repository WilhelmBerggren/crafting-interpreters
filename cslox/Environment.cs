using System;
using System.Collections.Generic;

namespace crafting_interpreters {
    public class Env {
        private Env enclosing { get; }
        private Dictionary<string, object> values = new Dictionary<string, object>();

        public Env() {
            enclosing = null;
        }

        public Env(Env enclosing) {
            this.enclosing = enclosing;
        }

        public void Define(string name, object value) {
            values.Add(name, value);
        }

        public object Get(Token name) {
            object res = null;
            if(values.ContainsKey(name.lexeme)) {
                values.TryGetValue(name.lexeme, out res);
                if(res == null) {
                    throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
                }
                return res;
            }
            if(enclosing != null) {
                return enclosing.Get(name);
            }
            return null;
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