using System;
using System.Runtime.Serialization;

namespace crafting_interpreters
{
    [Serializable]
    public class RuntimeError : SystemException
    {
        public Token token { get; }
        public RuntimeError(Token token, string message) : base(message)
        {
            this.token = token;
        }
    }
}