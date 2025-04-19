using G.Sharp.Compiler.CodeGen;
using G.Sharp.Compiler.Lexer;
using G.Sharp.Compiler.Parsers;

var code = """
               let name: string = "greg";
               let age: number = 33;
               let isTrue: bool = true;
               
               println name;
               println age;
               println isTrue;
               
               let variable:number = 10; variable = 20; println variable;                       
               println variable;
               
               let d: number = 10.13d;
               let m: number = 10.24m;
               let f: number = 10.87f;
               
               println d;
               println m;
               println f;
               
               let nums: number[] = [1 2 3];
               let names: string[] = ["Greg" "Felicio"];
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
           """;


var ifcode = """
             let n: number = 10;
             if n != 10 {
                 println "n is NOT 10 ❌";
             } else {
                 println "n is 10 ✅";
             }

             if 1 == 1 {
                 println "1 == 1 ✅";
             } else {
                 println "1 == 1 ❌";
             }

             let x: number = 7;
             if x == 7 {
                 println "x is 7 ✅";
             } else {
                 println "x is not 7 ❌";
             }

             let a: number = 10;
             if a >= 5 {
                 println "a >= 5 ✅";
             } else {
                 println "a >= 5 ❌";
             }

             if a <= 10 {
                 println "a <= 10 ✅";
             } else {
                 println "a <= 10 ❌";
             }

             let value: number = 3;
             if value < 5 {
                 println "value < 5 ✅";
             } else {
                 println "value < 5 ❌";
             }

             if value > 1 {
                 println "value > 1 ✅";
             } else {
                 println "value > 1 ❌";
             }

             let z: number = 8;
             if z == 10 {
                 println "z is 10 ❌";
             } else {
                 println "z is NOT 10 ✅";
             }

             let y: number = 20;
             if y > 10 {
                 println "y is big ✅";
             } else {
                 println "y is small ❌";
             }

             if y < 5 {
                 println "y is tiny ❌";
             } else {
                 println "y is not tiny ✅";
             }

             let b: number = 20;

             if a != b {
                 println "a != b ✅";
             } else {
                 println "a == b ❌";
             }

             if b >= a {
                 println "b >= a ✅";
             } else {
                 println "b >= a ❌";
             }

             if b <= a {
                 println "b <= a ❌";
             } else {
                 println "b <= a ❌";
             }

             if a == 10 {
                 println "a == 10 ✅";
             } else {
                 println "a == 10 ❌";
             }

             let isTrue: bool = true;
              if isTrue {
                  println "isTrue is true ✅";
             } 
             else {
                  println "isTrue is false ❌";
             }
              
             let isFalse: bool = false;
             if isFalse {
                  println "isFalse is true ❌";
             } 
             else {
                  println "isFalse is false ✅";
             }
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