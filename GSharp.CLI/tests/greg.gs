map list fn aux
    if len list > 0 then
        let h = head list
        let r = fn h
        let n = concat aux r
        let t = tail list
        map t fn n
    else
        aux
        
let arr = [1 2 3 4 5 6 7]

double x => x * 2

let e = []

let blabla = map arr double e

for a in blabla do
    println a
