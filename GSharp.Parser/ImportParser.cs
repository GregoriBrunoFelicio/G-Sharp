using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class ImportParser(Parser parser)
{
    public Expression Parse()
    {
        parser.Consume(TokenType.Import);
        var name = parser.Consume(TokenType.Identifier).Value;
        return new ImportDeclaration(name);
    }
}
