using G.Sharp.Compiler.AST;

namespace G.Sharp.Compiler.Parsers.Shared;

public interface IStatementParser
{
    Statement Parse();
}