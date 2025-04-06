namespace G.Sharp.Compiler.Lexer;

public enum TokenType
{
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

    //Interger
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
    
    LeftBracket,   // [
    RightBracket,  // ]
    
    LeftBrace,     // {
    RightBrace,    // }
    
}