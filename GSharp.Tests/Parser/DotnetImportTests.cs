using FluentAssertions;
using GSharp.AST;

namespace G.Sharp.Compiler.Tests.Parser;

// The dot in an import marks it as .NET interop: `import system.math` resolves a .NET type,
// while `import math` (no dot) stays a G# module import.
public class DotnetImportTests
{
    private static Expression FirstExpression(string source)
    {
        var tokens      = new GSharp.Lexer.Lexer(source).Tokenize();
        var expressions = new GSharp.Parser.Parser(tokens).Parse();
        return expressions[0];
    }

    [Fact]
    public void Dotted_Import_Becomes_A_Dotnet_Import()
    {
        var import = FirstExpression("import system.math") as DotnetImportDeclaration;

        import.Should().NotBeNull();
        import!.TypeName.Should().Be("system.math");
        import.Alias.Should().Be("math"); // lowercased last segment — the call-site name
    }

    [Fact]
    public void Plain_Import_Stays_A_Module_Import()
    {
        var import = FirstExpression("import math");

        import.Should().BeOfType<ImportDeclaration>();
        ((ImportDeclaration)import).ModuleName.Should().Be("math");
    }

    [Fact]
    public void Deeper_Namespace_Keeps_The_Full_Type_Name()
    {
        var import = FirstExpression("import system.text.stringbuilder") as DotnetImportDeclaration;

        import.Should().NotBeNull();
        import!.TypeName.Should().Be("system.text.stringbuilder");
        import.Alias.Should().Be("stringbuilder");
    }

    [Fact]
    public void As_Clause_Sets_The_Alias()
    {
        var import = FirstExpression("import system.array as arr") as DotnetImportDeclaration;

        import.Should().NotBeNull();
        import!.TypeName.Should().Be("system.array");
        import.Alias.Should().Be("arr"); // the alias replaces the default last-segment name
    }

    [Fact]
    public void Reserved_Word_Is_Allowed_In_The_Import_Path()
    {
        // `string` is a reserved keyword, but inside an import path it's just a name.
        var import = FirstExpression("import system.string as str") as DotnetImportDeclaration;

        import.Should().NotBeNull();
        import!.TypeName.Should().Be("system.string");
        import.Alias.Should().Be("str");
    }
}
