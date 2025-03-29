using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers.Shared;

namespace G.Sharp.Compiler.Parsers;

public class Parser(List<Token> tokens)
{
    private readonly ParserHelper _helper = new(tokens);

    public List<Statement> Parse()
    {
        var statements = new List<Statement>();

        while (IsNotEndOfFile())
        {
            var statement = ParseNextStatement();
            statements.Add(statement);
        }
        
        return statements;
    }
    
    private bool IsNotEndOfFile() =>
        _helper.Current < tokens.Count && tokens[_helper.Current].Type != TokenType.EndOfFile;

    private Statement ParseNextStatement()
    {
        if (_helper.Match(TokenType.Let))
            return new LetParser(_helper).Parse();

        if (_helper.Match(TokenType.Println))
            return new PrintParser(_helper).Parse();

        throw new Exception("Invalid statement");
    }
}