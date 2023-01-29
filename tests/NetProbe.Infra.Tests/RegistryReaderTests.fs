module RegistryTests

open System
open Xunit
open NetProbe.Core.Interfaces
open NetProbe.Infra.IO

let reader = RegistryReader () :> IRegistryReader

[<Fact>]
let ``read existing registry key with value test`` () =
    let value = reader.GetLocalKey @"SOFTWARE\Microsoft\Notepad\DefaultFonts" "lfFaceName"
    Assert.True(value.IsSome)

[<Fact>]
let ``read existing registry key without value test`` () =
    let value = reader.GetLocalKey @"SOFTWARE\Microsoft\Notepad\DefaultFonts" "lflflf"
    Assert.True(value.IsNone)

[<Fact>]
let ``none existing registry key returns none test`` () =
    let value = reader.GetLocalKey @"SOFTWARE\MicroMicroMicro\DeultFonts" "lfFaceName"
    Assert.True(value.IsNone)
