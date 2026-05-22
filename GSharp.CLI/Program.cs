using G.Sharp.Compiler;
using GSharp.CodeGen;
using GSharp.Lexer;
using GSharp.Parser;

 var code = GsFileReader.ReadSource("E:/Projects/G-Sharp/GSharp.CLI/hello.gs");

var lexer = new Lexer(code);
var tokens = lexer.Tokenize();

var parser = new Parser(tokens);
var expressions = parser.Parse();

var compiler = new Compiler();
compiler.CompileAndRun(expressions);