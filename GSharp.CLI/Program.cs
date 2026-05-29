using G.Sharp.Compiler;
using GSharp.CodeGen;
using GSharp.Lexer;
using GSharp.Parser;

try
{
    var path = args.Length > 0 ? args[0] : "tests.gs";
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
