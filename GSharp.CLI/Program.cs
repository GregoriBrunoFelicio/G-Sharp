using G.Sharp.Compiler;
using GSharp.CodeGen;

try
{
    var path = EntryResolver.ResolvePath(args);
    var expressions = GsLoader.ParseFile(path);
    var modules = GsLoader.LoadModules(path, expressions);

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