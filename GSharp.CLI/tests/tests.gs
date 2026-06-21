// recursive map: applies f to each element and prints the result
mymap f xs
    if array.empty xs then
        0
    else
        println f(array.head xs)
        mymap f (array.tail xs)

double x => x * 2
square x => x * x

nums -> [1 2 3 4 5]

mymap double nums
mymap square nums
