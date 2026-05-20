using G.Sharp.Compiler;
using GSharp.CodeGen;
using GSharp.Lexer;
using GSharp.Parser;

// var code = GsFileReader.ReadSource("E:/Projects/G-Sharp/GSharp.CLI/hello.gs");

var code =
    "let a = false\n" +
    "let b = true\n" +
    "let c = false\n" +
    "if a == true then\n" +
    "    println \"a is true\"\n" +
    "    if b == true then\n" +
    "        println \"b is true\"\n" +
    "        if c == true then\n" +
    "            println \"c is true\"\n" +
    "        else println \"c is false\"\n" +
    "    else println \"b is false\"\n" +
    "else println \"a is false\"\n";

var lexer = new Lexer(code);
var tokens = lexer.Tokenize();

var parser = new Parser(tokens);
var statements = parser.Parse();

var compiler = new Compiler();
// var count = 0;
// while (count <= 10000)
// {
compiler.CompileAndRun(statements);
//     count++;
// }