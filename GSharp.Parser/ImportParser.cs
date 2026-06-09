using GSharp.AST;
using GSharp.Lexer;

namespace GSharp.Parser;

public class ImportParser(Parser parser)
{
    // `import math`                   → G# module (loads math.gs)
    // `import system.math`            → .NET type (the dot marks it as interop)
    // `import system.string as str`   → .NET type with an alias used at call sites (str.concat)
    public Expression Parse()
    {
        parser.Consume(TokenType.Import);

        var firstSegment = ConsumeName();

        if (!parser.Check(TokenType.Dot))
            return new ImportDeclaration(firstSegment);

        return ParseDotnetImport(firstSegment);
    }

    private DotnetImportDeclaration ParseDotnetImport(string firstSegment)
    {
        var segments = new List<string> { firstSegment };

        while (parser.Match(TokenType.Dot))
            segments.Add(ConsumeName());

        var typeName = string.Join(".", segments);
        var alias    = ReadAliasOrDefault(segments[^1]);

        return new DotnetImportDeclaration(typeName, alias);
    }

    // The alias after `as` (e.g. `as str`), or the last segment when there's no alias.
    // An alias must be a plain identifier so it never clashes with a reserved word at call sites.
    private string ReadAliasOrDefault(string lastSegment)
    {
        var hasAlias = parser.Check(TokenType.As);
        if (!hasAlias)
            return lastSegment.ToLowerInvariant();

        parser.Advance(); // consume the contextual 'as'
        return parser.Consume(TokenType.Identifier).Value.ToLowerInvariant();
    }

    // Reads one dotted-path segment. Accepts reserved words too (e.g. `string`, `bool`), which
    // the lexer turns into keyword tokens — inside an import path they're just names. A name
    // token always starts with a letter, which is what distinguishes it from symbols like '.'.
    private string ConsumeName()
    {
        var token = parser.Current();

        var isName = token.Value.Length > 0 && char.IsLetter(token.Value[0]);
        if (!isName)
            throw new Exception($"{token.Line}: expected a name, got '{token.Value}'");

        parser.Advance();
        return token.Value;
    }
}
