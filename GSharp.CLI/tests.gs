double x => x * 2
apply f x => f(x)

println apply(double 5)

square x => x * x
applyTwice f x => f(f(x))

println applyTwice(double 3)
println applyTwice(square 2)

let fn = double
println fn(10)
