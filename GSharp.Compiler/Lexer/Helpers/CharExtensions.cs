namespace GSharp.Compiler.Lexer.Helpers;

public static class CharExtensions
{
    extension(char c)
    {
        public bool IsOnlyQuotes()
        {
            return c == '"';
        }

        public bool IsLetter()
        {
            return char.IsLetter(c);
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