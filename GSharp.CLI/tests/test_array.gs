main
    let nums = [1 2 3 4 5]
    println array.head nums
    println array.last nums
    println array.len nums
    println array.empty nums

    let t = array.tail nums
    println array.head t

    let r = array.reverse nums
    println array.head r

    let more = [6 7 8]
    let all = array.concat nums more
    println array.len all

    println string.from 42
