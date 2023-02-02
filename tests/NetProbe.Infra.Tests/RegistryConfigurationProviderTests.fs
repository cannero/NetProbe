module RegistryConfigurationProviderTests

open System.IO
open Xunit
open Microsoft.Extensions.Logging.Abstractions
open NetProbe.Core.Interfaces
open NetProbe.Infra.IO

module mock =
    let NoneRegistryReader () =
        { new IRegistryReader with
              member _.GetLocalKey _r _v =
                  None }

[<Fact>]
let ``parse config file works with correct schema test`` () =
    let hostname = "somehost"
    let content = $"<config><host>{hostname}</host></config>"

    let value = read.parseConfigFile (new NullLogger<RegistryConfigurationProvider>()) content

    Assert.Equal(hostname, value)

[<Fact>]
let ``not existing config file works throws exception test`` () =
    let target =
        RegistryConfigurationProvider (
            new NullLogger<_>(),
            "no reg key",
            "no value",
            "C:\notexisting",
            mock.NoneRegistryReader())
        :> IProbeConfigurationProvider
    
    let ex = Record.Exception(fun () -> box (target.Get()))

    Assert.IsType<IOException>(ex)
