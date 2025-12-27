let name = "greg";
let age = 33;
let isFoo = true;

println name;
println age;
println isFoo;
println 1 + 1 + 1 + 1;

let variable = 10;
variable = 20;
println variable;
println variable;

let d = 10.13d;
let m = 10.24m;
let f = 10.87f;

println d;
println m;
println f;

let nums = [1 2 3];
let names = ["Greg" "Felicio"];
let flags = [true false true];

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

let n = 10;
if n != 10 {
    println"n is NOT 10 ❌";
} else {
    println"n is 10 ✅";
}

if 1 == 1 {
    println"1 == 1 ✅";
} else {
    println"1 == 1 ❌";
}

let x = 7;
if x == 7 {
    println"x is 7 ✅";
} else {
    println"x is not 7 ❌";
}

let a = 10;
if a >= 5 {
    println"a >= 5 ✅";
} else {
    println"a >= 5 ❌";
}

if a <= 10 {
    println"a <= 10 ✅";
} else {
    println"a <= 10 ❌";
}

let value = 3;
if value < 5 {
    println"value < 5 ✅";
} else {
    println"value < 5 ❌";
}

if value > 1 {
    println"value > 1 ✅";
} else {
    println"value > 1 ❌";
}

let z = 8;
if z == 10 {
    println"z is 10 ❌";
} else {
    println"z is NOT 10 ✅";
}

let y = 20;
if y > 10 {
    println"y is big ✅";
} else {
    println"y is small ❌";
}

if y < 5 {
    println"y is tiny ❌";
} else {
    println"y is not tiny ✅";
}

let b = 20;

if a != b {
    println"a != b ✅";
} else {
    println"a == b ❌";
}

if b >= a {
    println"b >= a ✅";
} else {
    println"b >= a ❌";
}

if b <= a {
    println"b <= a ❌";
} else {
    println"b <= a ❌";
}

if a == 10 {
    println"a == 10 ✅";
} else {
    println"a == 10 ❌";
}

let isTrue = true;
if isTrue {
    println"isTrue is true ✅";
} else {
    println"isTrue is false ❌";
}

let isFalse = false;
if isFalse {
    println"isFalse is true ❌";
} else {
    println"isFalse is false ✅";
}

let counter = 0;
while counter < 5 {
    println counter;
    counter = counter + 1;
}

let i = 10;
while i > 0 {
    println i;
    i = i - 2;
}

let index = 1;
while index <= 3 {
    println index;
    index = index + 1;
}

let running = true;
let attempts = 0;
while running {
    println attempts;
    attempts = attempts + 1;
    if attempts == 3 {
        running = false;
    }
}

let step = 0;
while step < 5 and step != 3 {
    println step;
    step = step + 1;
}

let progress = 1;
while progress < 10 or progress > 100 {
    println progress;
    progress = progress + 3;
}

let score = 0;
while score < 3 {
    println score;
    score = score + 1;
}

let limit = 5;
let count = 0;
while count < limit and limit < 10 {
    println count;
    count = count + 1;
}

let flag = true;
let attempts2 = 0;
while flag and attempts2 < 2 {
    println attempts2;
    attempts2 = attempts2 + 1;
    if attempts2 == 2 {
        flag = false;
    }
}
