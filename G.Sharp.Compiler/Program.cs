using G.Sharp.Compiler;
using G.Sharp.Compiler.CodeGen;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers;


var code = GsFileReader.ReadSource("/home/greg/RiderProjects/G-SHARP/G.Sharp.Compiler/hello.gs");
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