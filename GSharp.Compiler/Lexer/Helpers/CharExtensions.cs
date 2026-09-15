namespace GSharp.Compiler.Lexer.Helpers;

public static class CharExtensions
{
    extension(char c)
    {
        public bool IsOnlyQuotes()
        {
            return c == '"';
        }

        public bool IsIdentifierStart()
        {
            return char.IsLetter(c) || c == '_';
        }

        public bool IsIdentifierPart()
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        public bool IsNumber()
        {
            return char.IsDigit(c);
        }

        public bool IsWhitespace()
        {
            return c is ' ' or '\t';
        }
    }
}