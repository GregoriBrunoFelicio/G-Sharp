# G♯ (GSharp)

**G♯** is a programming language that emits **IL (Intermediate Language)** and runs on the .NET runtime.  
I created this project to better understand how programming languages work internally — from lexing and parsing to IL code generation.  
Although I'm not a language design expert, I decided to build one from scratch to learn, explore, and share knowledge along the way.  
As a big fan of C#, I chose to implement everything in C# and gave the syntax a familiar feel.

> ⚠️ This project is in early development. Contributions and feedback are welcome!


---

## ✨ Current Features

At the moment, G♯ includes the following features:

- 🧠 Lexer and tokenization
- 🧱 Parser for basic statements
- ⚙️ IL (Intermediate Language) code generation using `System.Reflection.Emit`
- 🖨️ `printfln` for printing values with a newline
- 📦 Basic type support:
  - `number`
  - `string` (with double quotes)
- 🔤 Variable declarations using `let`

### Example (G# syntax)

```gsharp
let name: string = "Greg"
let count: number = 5

printfln name
printfln count

```

---

## 📁 Project Structure

```
GSharp/
├── G.Sharp.Compiler/
│   ├── AST/
│   │   ├── Statements.cs
│   │   ├── Type.cs
│   │   └── Values.cs
│   ├── Extensions/
│   │   └── CharExtensions.cs
│   ├── Lexer/
│   │   ├── Lexer.cs
│   │   ├── Syntax.cs
│   │   ├── Token.cs
│   │   └── TokenType.cs
│   ├── Compiler.cs
│   ├── Parser.cs
│   └── Program.cs
├── G.Sharp.Compiler.Tests/
│   └── Lexer/
│       ├── LexerStringTest.cs
│       └── LexerTests.cs
```

---

## 🛠️ Building

This project targets **.NET 9**. To build and run the compiler:

```bash
dotnet build
dotnet run --project G.Sharp.Compiler
```

To run the tests:

```bash
dotnet test
```

---

## ✅ Current Support

- [x] Lexer
- [x] Tokenization of strings, numbers, and keywords
- [x] Parser with support for `let` statements
- [x] String
- [x] Number
- [x] Boolean
- [x] Basic `printfln` output

---

## 🗂️ TODO

Planned features:

- [ ] Arithmetic operations (`+`, `-`, `*`, `/`)
- [ ] Conditionals (`if`, `else`)
- [ ] Loops (`for`, `while`)
- [ ] Functions and function calls
- [ ] REPL support ?

---

## 🧠 Goals

- Learn how to build a compiler and programming language from scratch
- Explore .NET internals, including IL generation and runtime behavior
- Experiment with language design and syntax

---

## 📬 Contact

If you have questions, suggestions, or just want to talk about language design and .NET internals, feel free to reach out:

📧 **gregory.wow@hotmail.com**

---

## 📄 License

MIT License. See [LICENSE](LICENSE) for more details.
```
