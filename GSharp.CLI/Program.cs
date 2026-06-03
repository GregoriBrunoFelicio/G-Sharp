using G.Sharp.Compiler;
using GSharp.CodeGen;
using GSharp.TypeChecker;

try
{
    var path        = EntryResolver.ResolvePath(args);
    var expressions = GsLoader.ParseFile(path);
    var modules     = GsLoader.LoadModules(path, expressions);

    // Type-check the program — throws on type errors before any IL is emitted.
    // The returned type map (expression → resolved type) will be passed to the
    // CodeGen once it is updated to emit typed IL instead of object-based IL.
    var _ = new TypeInferrer().Infer(expressions);

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
