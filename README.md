# G♯

G♯ is a purely functional programming language that emits IL (Intermediate Language) and runs on the .NET runtime.
It's a challenging project, but I'm learning a lot from it. I'm not a language design expert (yet), so you'll likely
find many rough edges and mistakes along the way, and that's totally fine.

This whole thing is meant to be fun, experimental, and educational.

⚠️ This project is in early development. Contributions and feedback are welcome!

---

## Architecture

```mermaid
flowchart TD
    SRC["SOURCE FILE (.gs)\nHuman-readable G# code — keywords, operators, indentation, literals"]

    subgraph LEXER["LEXER — Tokenization"]
        direction TB
        L1["IdentifierLexer — keywords and variable names"]
        L2["NumberLexer — integers, floats, decimals"]
        L3["StringLexer — string literals"]
        L4["SymbolLexer — operators, punctuation, parens, arrow (=>)"]
        L5["Indentation Tracking — BlockOpen / BlockClose tokens"]
    end

    T["List&lt;Token&gt; — If, Identifier, Then, BlockOpen, Number, LeftParen, Arrow, ..."]

    subgraph PARSER["PARSER — Syntax Analysis"]
        direction TB
        P1["LetParser — immutable variable bindings"]
        P2["IfParser — condition + then / else branches (inline or block)"]
        P3["ForParser / WhileParser — loop variable + body"]
        P4["FunctionParser — named functions (inline '=>' or indented block)"]
        P5["ExpressionParser — arithmetic, comparisons, logical ops, call expressions"]
    end

    AST["List&lt;Expression&gt; (AST) — IfExpression, ForExpression, LetExpression, FunctionDeclaration, ..."]

    subgraph CODEGEN["CODE GEN — IL Emission"]
        direction TB
        C1["ExpressionEmitter — central dispatch; every node leaves one value on the IL stack"]
        C2["IfEmitter / ForEmitter / WhileEmitter — control flow via IL labels"]
        C3["LetEmitter — local slots (immutable bindings only)"]
        C4["FunctionEmitter — two-pass compilation (Define + Emit); generates adapter methods"]
        C5["EmitContext — bundles locals, params, functions and adapters for all emitters"]
        C6["GSharpFunction — runtime wrapper for functions used as first-class values"]
        C7["RuntimeHelpers — numeric type promotion (int / float / double / decimal)"]
    end

    IL["IL Bytecode — System.Reflection.Emit / ILGenerator"]

    RT[".NET RUNTIME — JIT compiles IL to native code and executes it in memory"]

    OUT["OUTPUT"]

    SRC --> LEXER
    LEXER --> T
    T --> PARSER
    PARSER --> AST
    AST --> CODEGEN
    CODEGEN --> IL
    IL --> RT
    RT --> OUT
```

---

## Current Features

### Implemented

- Lexer and tokenization
- Parser for basic statements
- `println` for printing values
- Immutable variable bindings using `let` (purely functional — no reassignment)
- Dynamic type system
- Numeric literals: `int`, `long`, `double` (`d`), `float` (`f`), `decimal` (`m`)
- Conditionals (`if`, `else`) with `then` — inline or indented block
- Loops (`for`) with `do` — indented block
- Arrays (`[1 2 3]`) with `for` iteration
- User-defined functions — inline (`=>`) and indented block forms
- Function calls with arguments
- Recursion — functions can call themselves
- Higher-order functions — functions as first-class values (pass, store, return)

### Planned / Not Implemented Yet

- String concatenation with `+`
- Standard math functions (`abs`, `min`, `max`, `mod`)
- Multiple files / imports
- Error messages with line numbers

---

## Syntax

### Let Bindings

```gs
let num = 10
let name = "greg"
let isTrue = false
println name
```

---

### Numeric Literals

```gs
let i = 42
let d = 3.14d
let f = 2.5f
let m = 9.99m
```

---

### Arrays

```gs
let nums = [1 2 3 4 5]
let names = ["Alice" "Bob" "Carol"]
```

---

### Conditionals

```gs
# inline
if num >= 20 then println "X" else println "Y"

# block
if num >= 20 then
    println "X"
else
    println "Y"
```

---

### For

```gs
for item in nums do
    println item
```

---

### While

```gs
# while is deprecated — it requires mutable state and will be removed.
# Use recursion instead.
while num < 20 do
    println num
```

---

### Functions

Functions support two forms: **inline** (single expression after `=>`) and **block** (indented body).
The last expression in the body is the implicit return value.

```gs
# inline — single expression after =>
double(x) => x * 2

# inline with no parameters
greet() => println "Hello!"

# block form — last expression is the return value
max(a b)
    if a >= b then a else b

# calling a function
double(5)
greet()
max(3 7)
```

---

### Recursion

```gs
factorial(n)
    if n == 0 then 1 else n * factorial(n - 1)

fib(n)
    if n <= 1 then n else fib(n - 1) + fib(n - 2)

println factorial(10)
println fib(10)
```

---

### Higher-order Functions

Functions are first-class values — they can be passed as arguments and stored in bindings.

```gs
double(x) => x * 2
apply(f x) => f(x)
applyTwice(f x) => f(f(x))

println apply(double 5)       # 10
println applyTwice(double 3)  # 12

# storing a function in a binding
let fn = double
println fn(5)                 # 10
```

## Contact

If you have questions, suggestions, or just want to talk about language design and .NET internals, feel free to reach
out:

**gregory.wow@hotmail.com**

---

## MIT License

This project is licensed under the [MIT License](LICENSE).
