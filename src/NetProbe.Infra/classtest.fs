namespace classtest

type Someclass() =
    let _name = "sfsd"

    let privPlusOne(a) =
        a + 1

    member _.getName () =
        sprintf "%s %i" _name (privPlusOne(12))


