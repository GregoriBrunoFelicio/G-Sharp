using G.Sharp.Compiler;
using GSharp.CodeGen;
using GSharp.Lexer;
using GSharp.Parser;


 var code = GsFileReader.ReadSource("/home/greg/RiderProjects/G-SHARP/GSharp.CLI/hello.gs");


var lexer = new Lexer("println 1000 + 1 + 1 + 1;");
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