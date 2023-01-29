module LoopingTests

open System
open System.Threading
open Xunit
open Looping

let mutable x = 0
let fn () = x <- x + 1

let resetx () = x <- 0

[<Fact>]
let ``Loop is run multiple times test`` () =
    resetx ()
    let looper = Looper()
    looper.start fn
    Thread.Sleep(3 * loopf.looptime)
    looper.stop ()
    Assert.True(x >= 3 && x <= 4, (sprintf "x is %i" x))

[<Fact>]
let ``Loop does not run after stop test`` () =
    resetx ()
    let looper = Looper()
    looper.start fn
    looper.stop ()
    Thread.Sleep(3 * loopf.looptime)
    Assert.True(x <= 1)

[<Fact>]
let ``Loop cannot be started multiple times test`` () =
    resetx ()
    let looper = Looper()
    looper.start fn
    looper.start fn
    Thread.Sleep(3 * loopf.looptime)
    looper.stop ()
    Assert.True(x <= 4, (sprintf "x is %i" x))
