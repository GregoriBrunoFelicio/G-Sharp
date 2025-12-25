using G.Sharp.Compiler;
using GSharp.CodeGen;
using GSharp.Lexer;
using GSharp.Parser;

// var code = GsFileReader.ReadSource("/home/greg/RiderProjects/G-SHARP/GSharp.CLI/hello.gs");

var code = """
           let n = 10; println n;
           let nd = 10.1d; println nd;
           let nm = 10.1m; println nm;
           let nf = 10.1f; println nf;
           
           let name = "Greg"; println name;
           
           let array = [1 2 3];  for item in array { println item; }
           
           if n == 10 { println "ok"; }
           
           """;

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