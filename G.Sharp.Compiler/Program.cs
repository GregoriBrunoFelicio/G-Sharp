using G.Sharp;
using G.Sharp.Compiler;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers;

var code = """
           let nome: string = "greg";
           let idade: number = 33;
           println nome;
           println idade;
           """;

// var code = "let variable: string = \"10\"; println variable;";

var lexer = new Lexer(code);
var tokens = lexer.Tokenize();

var parser = new Parser(tokens);
var statements = parser.Parse();

var compiler = new Compiler();
var count = 0;
while (count <= 10000)
{
    compiler.CompileAndRun(statements);
    count++;
}