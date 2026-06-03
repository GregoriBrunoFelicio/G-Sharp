using G.Sharp.Compiler;
using GSharp.AST;
using GSharp.CodeGen;
using GSharp.Lexer;
using GSharp.Parser;

try
{
    string? path = args.Length switch
    {
        0                           => FindEntryPoint(),
        1 when args[0] == "run"     => FindEntryPoint(),
        1                           => args[0],
        _ when args[0] == "run"     => args[1],
        _                           => null
    };

    if (path is null)
    {
        Console.Error.WriteLine("usage: gs <file.gs>");
        Console.Error.WriteLine("       gs run [file.gs]");
        Environment.Exit(1);
    }

    var code = GsFileReader.ReadSource(path);

    var lexer = new Lexer(code);
    var tokens = lexer.Tokenize();

    var parser = new Parser(tokens);
    var expressions = parser.Parse();

    var baseDir = Path.GetDirectoryName(Path.GetFullPath(path))!;
    var modules = new Dictionary<string, List<Expression>>();

    var entryName = Path.GetFileNameWithoutExtension(path);

    foreach (var import in expressions.OfType<ImportDeclaration>())
    {
        if (import.ModuleName == entryName)
            throw new Exception($"'{import.ModuleName}' cannot import itself");

        var modulePath = Path.Combine(baseDir, import.ModuleName + ".gs");
        var moduleCode = GsFileReader.ReadSource(modulePath);
        var moduleTokens = new Lexer(moduleCode).Tokenize();
        modules[import.ModuleName] = new Parser(moduleTokens).Parse();
    }

    var compiler = new Compiler();
    compiler.CompileAndRun(expressions, modules);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(ex.Message);
    Console.ResetColor();
    Environment.Exit(1);
}

static string FindEntryPoint()
{
    var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.gs");
    return files.Length switch
    {
        0 => throw new Exception("no .gs files found in current directory"),
        1 => files[0],
        _ => throw new Exception("multiple .gs files found — specify which to run: gs <file.gs>")
    };
}
