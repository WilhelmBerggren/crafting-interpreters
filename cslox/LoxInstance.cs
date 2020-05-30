using System;
using System.Collections.Generic;

namespace crafting_interpreters
{
    public class LoxInstance
    {
        public LoxClass klass;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();
        public LoxInstance(LoxClass klass)
        {
            this.klass = klass;
        }

        public LoxInstance() { }

        public override string ToString()
        {
            return $"{klass.name} instance";
        }

        public object Get(Token name)
        {
            if (fields.ContainsKey(name.lexeme))
            {
                return fields[name.lexeme];
            }

            var method = klass.FindMethod(name.lexeme);
            if (method != null) return method.Bind(this);

            throw new RuntimeError(name, $"Undefined property '{name.lexeme}'.");
        }

        public void Set(Token name, object value)
        {
            if (fields.ContainsKey(name.lexeme))
            {
                fields[name.lexeme] = value;
            }
            else
            {
                fields.Add(name.lexeme, value);
            }
        }
    }
}