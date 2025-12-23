using G.Sharp.Compiler;
using GSharp.CodeGen;
using GSharp.Lexer;
using GSharp.Parser;

// var code = GsFileReader.ReadSource("/home/greg/RiderProjects/G-SHARP/GSharp.CLI/hello.gs");

var code = "let n = 10.13d; println n;";
//
// var code = "let array = [1 2 3];" +
//            "for item in array { println item; }";
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