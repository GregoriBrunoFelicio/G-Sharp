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

let firstName = "Greg"
let lastName = "Felicio"
println firstName + " " + lastName

let counter = 0
counter = counter + 1
counter = counter + 1
println counter

let nums = [1 2 3 4 5]
let names = ["Alice" "Bob" "Carol"]
let flags = [true false true]

for item in nums {
    println item
}

for n in names {
    println n
}

for flag in flags {
    println flag
}

let a = 10
let b = 20

if a == 10 {
    println"a == 10 ✅"
} else {
    println"a == 10 ❌"
}

if a != b {
    println"a != b ✅"
} else {
    println"a != b ❌"
}

if b > a {
    println"b > a ✅"
} else {
    println"b > a ❌"
}

if a < b {
    println"a < b ✅"
} else {
    println"a < b ❌"
}

if a >= 10 {
    println"a >= 10 ✅"
} else {
    println"a >= 10 ❌"
}

if b <= 20 {
    println"b <= 20 ✅"
} else {
    println"b <= 20 ❌"
}

let isTrue = true
let isFalse = false

if isTrue {
    println"isTrue ✅"
} else {
    println"isTrue ❌"
}

if isFalse {
    println"isFalse ❌"
} else {
    println"isFalse ✅"
}

let c = 0
while c < 5 {
    println c
    c = c + 1
}

let step = 0
while step < 5 and step != 3 {
    println step
    step = step + 1
}

let running = true
let attempts = 0
while running {
    println attempts
    attempts = attempts + 1
    if attempts == 3 {
        running = false
    }
}
