using G.Sharp.Compiler;
using GSharp.CodeGen;
using GSharp.Lexer;
using GSharp.Parser;

try
{
    string? path = args.Length switch
    {
        0 => null,
        1 => args[0],
        _ when args[0] == "run" => args[1],
        _ => null
    };

    if (path is null)
    {
        Console.Error.WriteLine("usage: gs <file.gs>");
        Console.Error.WriteLine("       gs run <file.gs>");
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
