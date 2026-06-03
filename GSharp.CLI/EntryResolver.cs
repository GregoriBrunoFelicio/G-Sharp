using GSharp.Lexer;
using G.Sharp.Compiler;

namespace G.Sharp.Compiler;

internal static class EntryResolver
{
    internal static string ResolvePath(string[] args)
    {
        var path = args.Length switch
        {
            0                           => FindEntryPoint(),
            1 when args[0] == "run"     => FindEntryPoint(),
            1                           => args[0],
            _ when args[0] == "run"     => args[1],
            _                           => null
        };

        if (path is not null) return path;

        Console.Error.WriteLine("usage: gs <file.gs>");
        Console.Error.WriteLine("       gs run [file.gs]");
        Environment.Exit(1);
        return null!;
    }

    private static string FindEntryPoint()
    {
        var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.gs");

        if (files.Length == 0)
            throw new Exception("no .gs files found in current directory");

        if (files.Length == 1)
            return files[0];

        var withMain = files.Where(FileHasMainDeclaration).ToArray();

        return withMain.Length switch
        {
            0 => throw new Exception(
                "multiple .gs files found — declare a 'main' function in one of them or specify a file: gs <file.gs>"),
            1 => withMain[0],
            _ => throw new Exception($"multiple entry points found: {string.Join(", ", withMain.Select(Path.GetFileName))}")
        };
    }

    private static bool FileHasMainDeclaration(string path)
    {
        var tokens = new Lexer(GsFileReader.ReadSource(path)).Tokenize();

        for (var i = 0; i < tokens.Count - 1; i++)
        {
            if (tokens[i].Type != TokenType.Identifier || tokens[i].Value != "main")
                continue;

            var j = i + 1;
            while (j < tokens.Count && tokens[j].Type == TokenType.Newline)
                j++;

            if (j < tokens.Count && tokens[j].Type is TokenType.BlockOpen or TokenType.Arrow)
                return true;
        }

        return false;
    }
}
