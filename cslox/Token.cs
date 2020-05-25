using System;

namespace crafting_interpreters
{
    public class Token
    {
        public Token(TokenType type, string lexeme, object literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }

        public TokenType type { get; set; }
        public string lexeme { get; set; }
        public Object literal { get; set; }
        public int line { get; set; }

        public override string ToString()
        {
            return $"{type} {lexeme} {literal}";
        }
    }
}