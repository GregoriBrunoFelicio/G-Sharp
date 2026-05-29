namespace GSharp.Lexer;

public record Token(TokenType Type, string Value, int Line = 0, int Column = 0);

public enum TokenType
{
    // Keywords
    Colon,
    Equals,
    Newline,
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
    Do,

    // Indentation
    BlockOpen,
    BlockClose,

    // Block openers
    Then,

    LeftParen,   // (
    RightParen,  // )

    LeftBracket, // [
    RightBracket, // ]

    LeftBrace, // {
    RightBrace, // }

    Arrow, // =>

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
    
    
    // Function
    Function
}