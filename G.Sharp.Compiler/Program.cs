using G.Sharp;
using G.Sharp.Compiler;
using G.Sharp.Compiler.CodeGen;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers;


// var code = """
//            let name: string = "greg";
//            let age: number = 33;
//            let isTrue: bool = true;
//            println name;
//            println age;
//            println isTrue;
//            """;

var code = "let variable: number = 10; variable = 20; println variable;";

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