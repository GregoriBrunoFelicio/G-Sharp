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
        L4["SymbolLexer — operators and punctuation"]
        L5["Indentation Tracking — BlockOpen / BlockClose tokens"]
    end

    T["List&lt;Token&gt; — If, Identifier, Then, BlockOpen, Number, ..."]

    subgraph PARSER["PARSER — Syntax Analysis"]
        direction TB
        P1["LetParser — immutable variable bindings"]
        P2["IfParser — condition + then / else branches (inline or block)"]
        P3["ForParser / WhileParser — loop variable + body"]
        P4["ExpressionParser — arithmetic, comparisons, logical operators"]
    end

    AST["List&lt;Statement&gt; (AST) — IfStatement, ForStatement, LetStatement, ..."]

    subgraph CODEGEN["CODE GEN — IL Emission"]
        direction TB
        C1["StatementEmitter — dispatches each node to the right emitter"]
        C2["ExpressionEmitter — pushes values onto the IL stack"]
        C3["IfEmitter / ForEmitter / WhileEmitter — control flow via IL labels"]
        C4["LetEmitter — local variable slots (immutable bindings only)"]
        C5["RuntimeHelpers — numeric type promotion (int / long / double)"]
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
- Conditionals (`if`, `else`) with `then` — inline or indented block
- Loops (`for`, `while`) with `do` — indented block

### In Progress / Not Implemented Yet

- Functions with parameters and return types
- Object types with constructor-based instantiation

---

This is the current plan for a first version of the language, a minimal but expressive set of features.

### Variable Declarations

```gsharp
let num = 10
let name = "Gregori"
let isTrue = false
println name
```

---

### Arrays

```gsharp
let array = [1 2 3 4 5 6 7 8 9 10]
array[10] = 90
```

---

### Conditionals

```gsharp
# inline
if num >= 20 then println "X" else println "Y"

# block
if num >= 20 then
    println "X"
else
    println "Y"
```

---

### While

```gsharp
# while exists in the grammar but is deprecated — it requires mutable state.
# It will be removed once recursion is implemented.
while num < 20 do
    println num
```

### For

```gsharp
for item in array do
    println item
```

### Functions (planned)

```gsharp
function Sum(a b) {
    return a + b
}

function Greet() {
    println "Hello!"
}
```

---

### Object with Constructor (planned)

```gsharp
object Person(name, age) {
    function SayHello() {
        println "Hello, my name is " + name
    }

    function IsAdult() {
        return age >= 18
    }
}

let p = new Person("Gregori", 20)
p.SayHello()
```

## Contact

If you have questions, suggestions, or just want to talk about language design and .NET internals, feel free to reach
out:

**gregory.wow@hotmail.com**

---

## MIT License

This project is licensed under the [MIT License](LICENSE).
