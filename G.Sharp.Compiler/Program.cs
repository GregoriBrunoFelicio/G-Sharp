using G.Sharp;
using G.Sharp.Compiler;
using G.Sharp.Compiler.CodeGen;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers;

//
// var code = """
//            let name: string = "greg";
//            let age: number = 33;
//            let isTrue: bool = true;
//            println name;
//            println age;
//            println isTrue;
//            """;

// var code = "let variable: number = 10; variable = 20; println variable;";

// var code =
//     "let nums: number[] = [1 2 3];";
//     // "let names: string[] = [\"Greg\" \"GPT\"];" +
//     // "let flags: bool[] = [true false true];";

var code = @"
    let nums: number[] = [1 2 3];
    let names: string[] = [""Greg"" ""Felicio""];
    let flags: bool[] = [true false true];
    for item in nums {
        println item;
    }
    for name in names {
        println name;
    }
    for flag in flags {
        println flag;
    }
    for item in [4 5 6] {
        println item;
    }
";
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