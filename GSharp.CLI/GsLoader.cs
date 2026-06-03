using GSharp.AST;
using GSharp.Lexer;
using GSharp.Parser;
using G.Sharp.Compiler;

namespace G.Sharp.Compiler;

internal static class GsLoader
{
    internal static List<Expression> ParseFile(string path)
    {
        var code = GsFileReader.ReadSource(path);
        var tokens = new Lexer(code).Tokenize();
        return new Parser(tokens).Parse();
    }

    internal static Dictionary<string, List<Expression>> LoadModules(string entryFilePath, List<Expression> expressions)
    {
        var projectRootDirectory = Path.GetDirectoryName(Path.GetFullPath(entryFilePath))!;
        var entryPointFileName = Path.GetFileNameWithoutExtension(entryFilePath);
        var moduleFilePathsByName = BuildModuleFilePathsByName(projectRootDirectory);
        var loadedModules = new Dictionary<string, List<Expression>>();
        var importQueue = new Queue<string>(expressions.OfType<ImportDeclaration>().Select(i => i.ModuleName));

        while (importQueue.Count > 0)
        {
            var moduleName = importQueue.Dequeue();

            if (moduleName == entryPointFileName)
                throw new Exception($"'{moduleName}' cannot import the entry point");

            if (loadedModules.ContainsKey(moduleName))
                continue;

            if (!moduleFilePathsByName.TryGetValue(moduleName, out var moduleFilePath))
                throw new Exception($"module '{moduleName}' not found");

            var moduleExpressions = ParseFile(moduleFilePath);
            loadedModules[moduleName] = moduleExpressions;

            foreach (var nestedImport in moduleExpressions.OfType<ImportDeclaration>())
                importQueue.Enqueue(nestedImport.ModuleName);
        }

        return loadedModules;
    }

    private static Dictionary<string, string> BuildModuleFilePathsByName(string projectRootDirectory)
    {
        var allGsFiles = Directory.GetFiles(projectRootDirectory, "*.gs", SearchOption.AllDirectories);

        var duplicateNames = allGsFiles
            .GroupBy(f => Path.GetFileNameWithoutExtension(f))
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        if (duplicateNames.Length > 0)
            throw new Exception($"duplicate module names found: {string.Join(", ", duplicateNames)}");

        return allGsFiles.ToDictionary(f => Path.GetFileNameWithoutExtension(f)!);
    }
}