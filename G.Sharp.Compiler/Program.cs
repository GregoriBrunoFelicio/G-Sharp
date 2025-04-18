using G.Sharp.Compiler.CodeGen;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers;

// var code = """
//                let name: string = "greg";
//                let age: number = 33;
//                let isTrue: bool = true;
//                
//                println name;
//                println age;
//                println isTrue;
//                
//                let variable:number = 10; variable = 20; println variable;                       
//                println variable;
//                
//                let d: number = 10.13d;
//                let m: number = 10.24m;
//                let f: number = 10.87f;
//                
//                println d;
//                println m;
//                println f;
//                
//                let nums: number[] = [1 2 3];
//                let names: string[] = ["Greg" "Felicio"];
//                let flags: bool[] = [true false true];
//                
//                for item in nums {
//                    println item;
//                }
//                for name in names {
//                    println name;
//                }
//                for flag in flags {
//                    println flag;
//                }
//                for item in [4 5 6] {
//                    println item;
//                }
//            """;

var code = "let nums: number[] = [1.1d 2.1d 3.1d]; for item in nums { println item; }";
// var code = "let n:number = 10.13d; println n;";

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