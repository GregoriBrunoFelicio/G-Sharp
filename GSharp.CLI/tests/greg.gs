sum list aux
    if len list > 0 then
        let h = head list
        let r = h + aux
        println r
        let t = tail list
        sum t aux
    else
        aux

let arr = [1 2 3 4 5 6 7]

// double x => x * 2

// let e = []

// let blabla = map arr double e

// for a in blabla do
    // println a
let g = sum arr 0

println g
