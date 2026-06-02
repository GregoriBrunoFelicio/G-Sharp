using G.Sharp.Compiler;
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

    var compiler = new Compiler();
    compiler.CompileAndRun(expressions);
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
    var dir = Directory.GetCurrentDirectory();

    var mainGs = Path.Combine(dir, "main.gs");
    if (File.Exists(mainGs)) return mainGs;

    var files = Directory.GetFiles(dir, "*.gs");
    return files.Length switch
    {
        0 => throw new Exception("no G# files found in current directory"),
        1 => files[0],
        _ => throw new Exception(
            "ambiguous: multiple .gs files found — run `gs <file.gs>` or create main.gs")
    };
}
