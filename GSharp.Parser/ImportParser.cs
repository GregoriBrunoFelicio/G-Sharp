using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class ImportParser(Parser parser)
{
    public ImportDeclaration Parse()
    {
        parser.Consume(TokenType.Import);
        var name = parser.Consume(TokenType.Identifier);
        return new ImportDeclaration(name.Value);
    }
}
