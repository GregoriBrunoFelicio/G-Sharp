using G.Sharp;
using G.Sharp.Compiler;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parser;

var code = """
           let nome1: string = "greg";
           let idade: number = 33;
           println nome;
           println idade;
           """;
 
// var code = "let verdadeiro: bool = true; println verdadeiro;";

var lexer = new Lexer(code);
var tokens = lexer.Tokenize();

var parser = new Parser(tokens);
var statements = parser.Parse();

var compiler = new Compiler();
compiler.CompileAndRun(statements);