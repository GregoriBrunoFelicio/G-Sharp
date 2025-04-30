namespace GSharp.Lexer;

public record Token(TokenType Type, string Value);

public enum TokenType
{
    // Keywords
    Colon,
    Equals,
    Semicolon,
    Println,
    EndOfFile,

    // Condition
    If,
    Else,

    //Let
    Let,
    Identifier,

    // Number
    Number,
    NumberLiteral,

    //String
    String,
    StringLiteral,

    //Boolean
    Boolean,
    BooleanTrueLiteral,
    BooleanFalseLiteral,

    //Loop
    For,
    In,
    While,

    LeftBracket, // [
    RightBracket, // ]

    LeftBrace, // {
    RightBrace, // }

    // Comparison
    GreaterThan, // >
    LessThan, // <
    GreaterThanOrEqual, // >=
    LessThanOrEqual, // <=
    NotEqual, // !=
    EqualEqual, // ==

    // Logical
    And, // and
    Or, // or
    Not, // not

    // Operators
    Plus, // +
    Minus, // -
    Multiply, // *    
    Divide, // /
}