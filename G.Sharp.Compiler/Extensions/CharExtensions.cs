
namespace G.Sharp.Compiler.Extensions;
public static class CharExtensions
{
    public static bool IsOnlyQuotes(this char c) => c == '"';
    public static bool IsLetter(this char c) => char.IsLetter(c);
    public static bool IsNumber(this char c) => char.IsDigit(c);
    public static bool IsWhitespace(this char c) => char.IsWhiteSpace(c);
}