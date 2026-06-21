map array fn aux
    if array.empty array then
        aux
    else
        head -> array.head array
        tail -> array.tail array
        result -> fn head
        map tail fn (array.concat result aux)

arr -> [1 2 3 4 5 6]
double x => x * 2

println (map arr double [])
