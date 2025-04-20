using FluentAssertions;

namespace G.Sharp.Compiler.Tests.Parser;

public class GsFileReaderTests
{
    [Fact]
    public void Should_Read_Valid_Gs_File()
    {
        var path = CreateTempFile("valid.gs", "let x: number = 10\r\nprintln x");

        var content = GsFileReader.ReadSource(path);

        content.Should().Be("let x: number = 10\nprintln x");
    }

    [Fact]
    public void Should_Throw_When_Path_Is_Null()
    {
        var act = () => GsFileReader.ReadSource(null!);

        act.Should().Throw<ArgumentException>().WithMessage("*File path cannot be null or empty*");
    }

    [Fact]
    public void Should_Throw_When_Path_Is_Empty()
    {
        var act = () => GsFileReader.ReadSource("");

        act.Should().Throw<ArgumentException>().WithMessage("*File path cannot be null or empty*");
    }

    [Fact]
    public void Should_Throw_When_File_Does_Not_Exist()
    {
        var path = Path.Combine(Path.GetTempPath(), "nonexistent.gs");

        var act = () => GsFileReader.ReadSource(path);

        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void Should_Throw_When_Extension_Is_Not_Gs()
    {
        var path = CreateTempFile("invalid.txt", "print('hi')");

        var act = () => GsFileReader.ReadSource(path);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Invalid file extension*");
    }

    [Fact]
    public void Should_Throw_When_File_Is_Empty()
    {
        var path = CreateTempFile("empty.gs", "");

        Action act = () => GsFileReader.ReadSource(path);

        act.Should().Throw<InvalidDataException>().WithMessage("*is empty or contains only whitespace*");
    }

    [Fact]
    public void Should_Throw_When_File_Contains_Only_Whitespace()
    {
        var path = CreateTempFile("whitespace.gs", "   \n\t");

        var act = () => GsFileReader.ReadSource(path);

        act.Should().Throw<InvalidDataException>();
    }

    private string CreateTempFile(string name, string content)
    {
        var path = Path.Combine(Path.GetTempPath(), name);
        File.WriteAllText(path, content);
        return path;
    }
}