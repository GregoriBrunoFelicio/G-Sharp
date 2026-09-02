# G♯

[![Build](https://github.com/GregoriBrunoFelicio/G-Sharp/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/GregoriBrunoFelicio/G-Sharp/actions/workflows/dotnet.yml)
[![License: MIT](https://img.shields.io/github/license/GregoriBrunoFelicio/G-Sharp)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)
[![Status](https://img.shields.io/badge/status-active%20development-blue)](#current-features)

G♯ is a purely functional programming language that compiles to .NET IL and runs on the .NET runtime.
Everything is an expression, all bindings are immutable, and there is no reassignment.

Early-stage but under active development. Not yet production-ready.

---

## Contents

- [Getting Started](#getting-started)
- [Syntax](#syntax)
- [Standard library](#standard-library)
- [Functions](#functions)
- [Lambda expressions](#lambda-expressions)
- [Entry point](#entry-point)
- [Modules and imports](#modules-and-imports)
- [Language Server](#language-server)
- [Current Features](#current-features)
- [Architecture](#architecture)
- [Contact](#contact)
- [License](#license)

---

## Getting Started

### Install the CLI

```bash
dotnet tool install -g --add-source ./nupkg GSharp.CLI
```

### Run a file

```bash
gs run main.gs     # explicit
gs run             # auto-detects entry point in the current directory
gs hello.gs        # shorthand
```

See [`GSharp.CLI/tests/hello.gs`](GSharp.CLI/tests/hello.gs) for a file that exercises the whole
language end to end.

### Rebuild and reinstall

```bash
./update-tool.sh   # rebuilds GSharp.CLI and reinstalls gs
./update-lsp.sh     # rebuilds the language server and reinstalls gsharp-lsp
```

---

## Syntax

### Bindings

Bindings are immutable. There is no reassignment.

```gs
name -> "Alice"
age  -> 30
pi   -> 3.14d
println name
```

### Numeric types

```gs
i -> 42       // int
d -> 3.14d    // double
f -> 2.5f     // float
m -> 9.99m    // decimal
```

### Booleans

```gs
yes -> true
no  -> false
```

### Arrays

```gs
nums  -> [1 2 3 4 5]
names -> ["Alice" "Bob" "Carol"]
```

### Conditionals

`if` is an expression — it can appear on the right side of a binding.

```gs
// inline
if age >= 18 then println "adult" else println "minor"

// block
if age >= 18 then
    println "adult"
else
    println "minor"

// as expression
label -> if age >= 18 then "adult" else "minor"
println label
```

### Logical operators

`and` and `or` are binary operators with short-circuit evaluation.

```gs
a -> true
b -> false

both   -> a and b     // false
either -> a or b      // true

if a and age >= 18 then
    println "adult and confirmed"
```

### Unary operators

`not` (or `!`) negates a boolean; `-` negates a number. Both bind tighter than any binary
operator and looser than function application.

```gs
done -> false
println not done      // true
println !done         // true, same as `not`

x -> 10
println -x            // -10
println -x + 3        // -7, unary binds before +
```

`f -x` still means `f - x` (binary subtraction) — juxtaposing a call with a unary operand
requires parens: `f (-x)`.

### For — functional map

`for` transforms a collection and returns a new array.
The last expression in the body is the value for each element.

```gs
nums    -> [1 2 3 4 5]
doubled -> for item in nums do
    item * 2

for x in doubled do
    println x    // 2 4 6 8 10
```

### Comments

```gs
// this is a comment
x -> 10    // inline comment
```

---

## Standard library

G# ships a built-in standard library. No import needed — all functions are always available.

### array

```gs
nums -> [1 2 3 4 5]

println array.head nums        // 1
println array.last nums        // 5
println array.len nums         // 5
println array.empty nums       // False

rest     -> array.tail nums            // [2 3 4 5]
reversed -> array.reverse nums         // [5 4 3 2 1]
sorted   -> array.sort [3 1 2]         // [1 2 3]
more     -> [6 7 8]
all      -> array.concat nums more     // [1 2 3 4 5 6 7 8]

doubled -> array.map nums (n => n * 2)             // [2 4 6 8 10]
evens   -> array.filter nums (n => n > 2)          // [3 4 5]
sum     -> array.fold nums 0 (acc n => acc + n)    // 15
```

### string

```gs
println string.from 42       // "42"
println string.from 3.14d    // "3.14"
```

---

## Functions

No parentheses in definitions. `=>` always marks where the body starts — followed by either a
single expression (inline) or an indented block. The last expression in a block is the implicit
return value.

```gs
// inline
double x => x * 2
add a b  => a + b
greet    => println "Hello!"

// block — last expression is returned
max a b =>
    if a >= b then a else b
```

### Function calls

No parentheses needed when arguments are simple values (literals or variable names).
Parentheses are required when an argument is an expression.

```gs
println double 5        // 10
println add 3 7         // 10
println max 100 42      // 100

// parentheses required for expression arguments
// `factorial n - 1` would parse as `(factorial n) - 1` — wrong
factorial n =>
    if n == 0 then 1 else n * factorial(n - 1)

println factorial 10    // 3628800
```

### Recursion

```gs
fib n =>
    if n <= 1 then n else fib(n - 1) + fib(n - 2)

println fib 10    // 55
```

### Higher-order functions

Functions are first-class values — pass them, store them, return them.

```gs
double x       => x * 2
apply f x      => f(x)
applyTwice f x => f(f(x))

println apply double 5        // 10
println applyTwice double 3   // 12

fn -> double
println fn(10)                // 20
```

---

## Lambda expressions

`=>` always marks where a function body starts, whether the function is named or anonymous — the
**only** difference is whether a name comes before the parameter list.

```gs
// named function — a name, then parameters, then =>
square x => x * x

// lambda (anonymous function) — no name, just parameters then =>
squareLambda -> x => x * x
```

`square` above can be called directly (`square 5`). A lambda has no name of its own — it only
becomes usable once you bind it (`squareLambda -> x => x * x`) or pass it straight into a call.
As a call argument it must be parenthesized:

```gs
apply x f => f x
println (apply 5 (n => n * 2))           // 10

nums  -> [1 2 3 4 5]
evens -> array.filter nums (n => n > 2)  // [3 4 5]
```

Lambdas are non-capturing — a lambda body can only see its own parameters, not bindings from
an enclosing function or lambda.

---

## Entry point

For a single-file program, the file itself is the entry point — everything runs top-to-bottom.

For multi-file programs, declare `main` as the entry point. Exactly one file must have `main`.

```gs
add a b => a + b

main
    result -> add 10 20
    println result
```

---

## Modules and imports

A module is any `.gs` file without a `main` declaration. Import it by name — no path needed.
Module names must be unique across the project.

```gs
// mathutils.gs
add a b => a + b
square x => x * x
```

```gs
// main.gs
import mathutils

main
    println mathutils.add 3 5      // 8
    println mathutils.square 4     // 16
```

Module files can be placed in any subdirectory. The compiler finds them by name.
Modules can import other modules — circular imports are detected and reported as errors.

---

## Language Server

G# ships a Language Server Protocol (LSP) implementation that integrates with any LSP-compatible editor.

### Install

```bash
dotnet tool install -g --add-source ./nupkg GSharp.LanguageServer
```

The server binary is `gsharp-lsp`.

### Features

**Hover** — hover over any binding, literal, or function call to see its inferred type.

```
x -> 42           →  int
d -> 3.14d        →  double
add a b => a + b  →  (int → (int → int))
```

**Diagnostics** — type errors and parse errors are reported inline as you type, before you run the program.

---

## Current Features

| Feature | Status |
|---|---|
| Immutable bindings (`x -> value`) | ✅ |
| Numeric types (int, float, double, decimal) | ✅ |
| Strings | ✅ |
| Booleans (`true`, `false`) | ✅ |
| Logical operators (`and`, `or`, short-circuit) | ✅ |
| Unary operators (`not`/`!`, `-`) | ✅ |
| Constant folding (literal arithmetic evaluated at compile time) | ✅ |
| Arrays | ✅ |
| `if/else` as expression (inline and block) | ✅ |
| `for` as functional map (returns array) | ✅ |
| Named functions (inline `=>` and block) | ✅ |
| No-paren function calls | ✅ |
| Recursion | ✅ |
| Higher-order functions | ✅ |
| Line comments (`//`) | ✅ |
| `main` as entry point | ✅ |
| `gs run` CLI with auto-detection | ✅ |
| Standard library (`array.*`, `string.*`) | ✅ |
| Module import system (`import`, dot notation) | ✅ |
| Multi-file projects with recursive module loading | ✅ |
| Circular import detection | ✅ |
| Hindley-Milner type inference | ✅ |
| Language server (hover, diagnostics) | ✅ |
| Tail-call optimization (self-recursion) | ✅ |
| Native typed function parameters | ✅ |
| Lambda expressions (non-capturing) | ✅ |
| `map` / `filter` / `fold` | ✅ |
| Pattern matching | ⏳ |
| Custom types (records, ADTs) | ⏳ |

---

## Architecture

G# is a straight-line pipeline: Lexer → Parser → ConstantFolder → TypeInferrer → CodeGen, all
living inside one project, `GSharp.Compiler`, with a folder per layer.

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full pipeline diagram, the project
layout, and a deep dive into the Hindley-Milner type checker and tail-call optimization.

---

## Contact

**gregory.wow@hotmail.com**

---

## License

[MIT](LICENSE)
