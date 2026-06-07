
namespace GSharp.Lexer.Helpers;
public static class CharExtensions
{
    extension(char c)
    {
        public bool IsOnlyQuotes() => c == '"';
        public bool IsLetter() => char.IsLetter(c);
        public bool IsNumber() => char.IsDigit(c);
        public bool IsWhitespace() => c is ' ' or '\t';
    }
}
