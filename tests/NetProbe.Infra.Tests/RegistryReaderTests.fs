module RegistryTests

open System
open Xunit
open RegistryReader

[<Fact>]
let ``read existing registry key with value test`` () =
    let value = openLocal @"SOFTWARE\Microsoft\Notepad\DefaultFonts" "lfFaceName"
    Assert.True(value.IsSome)
    Assert.False((value = Some(ValueNotFound)))

[<Fact>]
let ``read existing registry key without value test`` () =
    let value = openLocal @"SOFTWARE\Microsoft\Notepad\DefaultFonts" "lflflf"
    Assert.True((value = Some(ValueNotFound)))

[<Fact>]
let ``none existing registry key returns none test`` () =
    let value = openLocal @"SOFTWARE\MicroMicroMicro\DeultFonts" "lfFaceName"
    Assert.True(value.IsNone)
