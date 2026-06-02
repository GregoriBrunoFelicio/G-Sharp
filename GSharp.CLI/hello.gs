let name = "greg"
let age = 33
let isActive = true

println name
println age
println isActive

let i = 42
let d = 3.14d
let f = 2.5f
let m = 9.99m

println i
println d
println f
println m

let x = 10
let y = 3

println x + y
println x - y
println x * y
println x / y

let nums = [1 2 3 4 5]
let names = ["Alice" "Bob" "Carol"]
let flags = [true false true]
b
for item in nums do
    println item

for n in names do
    println n

for flag in flags do
    println flag

let a = 10
let b = 20

if a == 10 then println "a == 10" else println "a != 10"

if a != b then println "a != b" else println "a == b"

if b > a then println "b > a" else println "b <= a"

if a < b then println "a < b" else println "a >= b"

if a >= 10 then println "a >= 10" else println "a < 10"

if b <= 20 then println "b <= 20" else println "b > 20"

let isTrue = true
let isFalse = false

if isTrue then println "isTrue is true" else println "isTrue is false"

if isFalse then println "isFalse is true" else println "isFalse is false"

if a == 10 then
    println "block: a is 10"
else
    println "block: a is not 10"

if b > a then
    println "block: b is greater"

let label = if a > 5 then "big" else "small"
println label

add a b => a + b
square x => x * x
greet => println "Hello from G#!"

greet()
println add(3 5)
println square(4)

max a b
    if a >= b then a else b

println max(10 7)
println max(3 99)

factorial n
    if n == 0 then 1 else n * factorial(n - 1)

fib n
    if n <= 1 then n else fib(n - 1) + fib(n - 2)

println factorial(5)
println factorial(10)
println fib(10)

double x => x * 2
apply f x => f(x)
applyTwice f x => f(f(x))

println apply(double 5)
println applyTwice(double 3)

let fn = double
println fn(10)

// --- call syntax ---

// without parentheses: works when arguments are simple values (literals or variable names)
println add x y         // 13

// parentheses are required when an argument is an expression, not a simple value
// `add x + 1 y` parses as `(add x) + 1` — the `+ 1` falls outside the call
// wrap the expression in parentheses to pass it as a single argument
println add(x + 1 y)    // 14
