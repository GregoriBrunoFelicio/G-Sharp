// ============================================================
// hello.gs — smoke test for all G# language features
// Run this file after any change to verify nothing is broken.
// ============================================================

// --- let bindings ---
let name     = "Greg"
let age      = 33
let isActive = true

println name
println age
println isActive

// --- numeric types ---
let i = 42
let d = 3.14d
let f = 2.5f
let m = 9.99m

println i
println d
println f
println m

// --- arithmetic ---
let x = 10
let y = 3

println x + y
println x - y
println x * y
println x / y

// --- arrays ---
let nums  = [1 2 3 4 5]
let names = ["Alice" "Bob" "Carol"]
let flags = [true false true]

// --- for as side effect ---
for item in nums do
    println item

for n in names do
    println n

for flag in flags do
    println flag

// --- for as functional map (returns new array) ---
let doubled = for item in nums do
    item * 2

for x in doubled do
    println x

// --- built-in array functions ---
println head nums
println last nums
println len  nums
println empty nums
println nth  nums 2

let rest     = tail nums
let reversed = reverse nums
let more     = [6 7 8]
let all      = concat nums more

println head rest
println head reversed
println len all

// --- type conversion ---
println str 42
println str 3.14d
println str true

// --- conditionals ---
let a = 10
let b = 20

if a == 10 then println "a == 10" else println "a != 10"
if a != b  then println "a != b"  else println "a == b"
if b > a   then println "b > a"   else println "b <= a"
if a < b   then println "a < b"   else println "a >= b"
if a >= 10 then println "a >= 10" else println "a < 10"
if b <= 20 then println "b <= 20" else println "b > 20"

let isTrue  = true
let isFalse = false

if isTrue  then println "isTrue is true"   else println "isTrue is false"
if isFalse then println "isFalse is true"  else println "isFalse is false"

// block form
if a == 10 then
    println "block: a is 10"
else
    println "block: a is not 10"

if b > a then
    println "block: b is greater"

// if as expression
let label = if a > 5 then "big" else "small"
println label

// --- functions: inline ---
add    p q => p + q
square p   => p * p
greet      => println "Hello from G#!"

greet()
println add(3 5)
println square(4)

// --- functions: block ---
max p q
    if p >= q then p else q

println max(10 7)
println max(3 99)

// --- recursion ---
factorial n
    if n == 0 then 1 else n * factorial(n - 1)

fib n
    if n <= 1 then n else fib(n - 1) + fib(n - 2)

println factorial 5
println factorial 10
println fib 10

// --- higher-order functions ---
double   p   => p * 2
apply    f p => f(p)
applyTwice f p => f(f(p))

println apply double 5
println applyTwice double 3

let fn = double
println fn(10)

// --- call syntax: no parens for simple args ---
println add x y

// --- call syntax: parens required for expression args ---
println add(x + 1 y)
