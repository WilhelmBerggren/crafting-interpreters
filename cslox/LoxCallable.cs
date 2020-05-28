using System;
using System.Collections.Generic;

namespace crafting_interpreters {
    interface ILoxCallable {
        int Arity();
        object Call(Interpreter interpreter, List<object> args);
    }

    class Clock : ILoxCallable
    {
        public int Arity()
        {
            return 0;
        }

        public object Call(Interpreter interpreter, List<object> args)
        {
            return (double)((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();
        }

        public override string ToString() {
            return "<native fn>";
        }
    }
}