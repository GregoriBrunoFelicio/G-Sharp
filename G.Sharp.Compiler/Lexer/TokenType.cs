namespace G.Sharp.Compiler.Lexer;

public enum TokenType
{
    Colon,
    Equals,
    Semicolon,
    Println,
    EndOfFile,

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
}