# G♯

One day I simply woke up and thought:
“I want to create my own programming language.” 

G♯ is a programming language that emits IL (Intermediate Language) and runs on the .NET runtime.
It’s a challenging project, but I’m learning a lot from it. I’m not a language design expert (yet), so you’ll likely
find many rough edges and mistakes along the way — and that’s totally fine.

This whole thing is meant to be fun, experimental, and educational.

⚠️ This project is in early development. Contributions and feedback are welcome!

---

## ✨ Current Features

### ✅ Implemented

- 🧠 Lexer and tokenization
- 🧱 Parser for basic statements
- 🖨️ `println` for printing values
- 🔤 Variable declarations using `let`
- 📦 Type support: `number`, `string`, `bool`
- 🔁 Conditionals (`if`, `else`) with `{}` blocks
- 🔂 Loops (`for`, `while`) using `{}`

### 🚧 In Progress / Not Implemented Yet

- 🧩 Functions with parameters and return types
- 🧱 Object types with constructor-based instantiation
---

## 🧪 Syntax Examples

Defining a good syntax has been one of the biggest challenges in this journey.  
I now understand why some languages made syntax decisions I once judged harshly.  
It's not easy — especially when you also have to think about performance, IL generation, and how every small choice
might impact memory, the runtime, or even GC pressure. A decision made just to make code look "pretty" can have deep
architectural consequences.

This is the current plan for a first version of the language — a minimal but expressive set of features.  
✅ means it's already implemented, and 🛠️ means it's planned but not working yet.

### Variable Declarations ✅

```gsharp
let num: number = 10;
let name: string = "Gregori";
let isTrue: bool = false;
num = 20;
println name;
```

---

### Arrays ✅

```gsharp
let array: number[] = [1 2 3 4 5 6 7 8 9 10];
array[10] = 90;
```

---

### Conditionals ✅

```gsharp
if num >= 20 {
    println "X";
} else {
    println "Y";
}
```

---

### 🛠️ While ✅

```gsharp
while num < 20 {
    num++;
}
```

### 🛠️ For ✅

```gsharp
for item in array {
    println item;
}
```
### 🛠️ Functions (planned)

```gsharp
function Sum(a: number, b: number): number {
    return a + b;
}

function Greet(): void {
    println "Hello!";
}
```

---

### 🛠️ Object with Constructor (planned)

```gsharp
object Person(name: string, age: number) {
    function SayHello(): void {
        println "Hello, my name is " + name;
    }

    function IsAdult(): bool {
        return age >= 18;
    }
}

let p: Person("Gregori", 20);
p.SayHello();
```

## 📬 Contact

If you have questions, suggestions, or just want to talk about language design and .NET internals, feel free to reach
out:

📧 **gregory.wow@hotmail.com**

---

## 📄 MIT License

This project is licensed under the [MIT License](LICENSE).

