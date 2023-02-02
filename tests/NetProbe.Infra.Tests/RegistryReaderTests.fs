module RegistryTests

open Xunit
open NetProbe.Core.Interfaces
open NetProbe.Core.ValueObjects
open NetProbe.Infra.IO

let reader = RegistryReader () :> IRegistryReader

let createConfig key name =
    { Key = key; ValueName = name; FallbackPath = ""; }

[<Fact>]
let ``read existing registry key with value test`` () =
    let value = createConfig  @"SOFTWARE\Microsoft\Notepad\DefaultFonts" "lfFaceName"
                |> reader.GetLocalKey
    Assert.True(value.IsSome)

[<Fact>]
let ``read existing registry key without value test`` () =
    let value = createConfig @"SOFTWARE\Microsoft\Notepad\DefaultFonts" "lflflf"
                |> reader.GetLocalKey
    Assert.True(value.IsNone)

[<Fact>]
let ``none existing registry key returns none test`` () =
    let value = createConfig @"SOFTWARE\MicroMicroMicro\DeultFonts" "lfFaceName"
                |> reader.GetLocalKey
    Assert.True(value.IsNone)
