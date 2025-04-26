using System.Text;

namespace G.Sharp.Compiler;

public static class GsFileReader
{
    public static string ReadSource(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("File path cannot be null or empty.", nameof(path));

        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found: {path}");

        if (Path.GetExtension(path) != ".gs")
            throw new InvalidOperationException($"Invalid file extension. Expected '.gs' but got '{Path.GetExtension(path)}'.");

        var source = File.ReadAllText(path, Encoding.UTF8);

        if (string.IsNullOrWhiteSpace(source))
            throw new InvalidDataException($"File '{path}' is empty or contains only whitespace.");

        return NormalizeLineEndings(source);
    }

    private static string NormalizeLineEndings(string input) => 
        input.Replace("\r\n", "\n").Replace('\r', '\n');
}