using G.Sharp.Compiler;
using GSharp.Compiler.CodeGen;
using GSharp.Compiler.Optimizer;
using GSharp.Compiler.TypeChecker;

try
{
    var path = EntryResolver.ResolvePath(args);
    var parsedExpressions = GsLoader.ParseFile(path);
    var foldedExpressions = ConstantFolder.FoldAll(parsedExpressions);
    var modules = GsLoader.LoadModules(path, foldedExpressions)
        .ToDictionary(module => module.Key, module => ConstantFolder.FoldAll(module.Value));

    var typeMap = new TypeInferrer().Infer(foldedExpressions);

    var compiler = new Compiler();
    compiler.CompileAndRun(foldedExpressions, modules, typeMap);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(ex.Message);
    Console.ResetColor();
    Environment.Exit(1);
}