let nums = [1 2 3 4 5]

// for as map: returns a new array
let doubled = for item in nums do
    item * 2

for x in doubled do
    println x

// side effects still work — result is [null null null]
for item in nums do
    println item
