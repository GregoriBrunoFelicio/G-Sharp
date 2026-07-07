namespace GSharp.Lexer;

public record Token(TokenType Type, string Value, int Line = 0, int Column = 0);

public enum TokenType
{
    // Keywords
    Equals, // bare '=' — invalid in G#; kept so the lexer can gate '==' and '=>'
    Newline,
    Println,
    EndOfFile,

    // Condition
    If,
    Else,

    // Identifier
    Identifier,

    // Number
    NumberLiteral,

    //String
    StringLiteral,

    //Boolean
    BooleanTrueLiteral,
    BooleanFalseLiteral,

    //Loop
    For,
    In,
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

    Arrow,     // =>
    ThinArrow, // ->

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
    Function,

    // Imports
    Import,
    Dot,
    As
}