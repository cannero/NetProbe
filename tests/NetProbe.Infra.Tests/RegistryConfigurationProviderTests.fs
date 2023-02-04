module RegistryConfigurationProviderTests

open System.IO
open Xunit
open Microsoft.Extensions.Logging.Abstractions
open NetProbe.Core.Interfaces
open NetProbe.Core.ValueObjects
open NetProbe.Infra.IO

module mock =
    let NoneRegistryReader () =
        { new IRegistryReader with
              member _.GetLocalKey _c =
                  None }

    let nullLogger =
        new NullLogger<RegistryConfigurationProvider>()

module files =
    let createConfigFile path host port =
        File.WriteAllText(path, $"<config><host>{host}</host><port>{port}</port></config>")

    let createWrongConfigFile path =
        File.WriteAllText(path, $"<config><nothost>just some value</nothost><port>2039</port></config>")

    let deleteConfigFile path =
        File.Delete(path)

let target regkey regvalue fallback =
    RegistryConfigurationProvider (
            new NullLogger<_>(),
            { Key = regkey; ValueName = regvalue; FallbackPath = fallback;} ,
            mock.NoneRegistryReader())
        :> IProbeConfigurationProvider

[<Fact>]
let ``parse config file works with correct schema test`` () =
    let hostname = "somehost"
    let port = 9381u
    let content = $"<config><host>{hostname}</host><port>{port}</port></config>"

    let hostAct, portAct = read.parseConfigFile mock.nullLogger content

    Assert.Equal(hostname, hostAct.Value)
    Assert.Equal(port, portAct.Value)

[<Fact>]
let ``parse config file returns None without correct schema test`` () =
    let content = $"<config><nohostdefined>somevalue</nohostdefined></config>"

    let host, port = read.parseConfigFile mock.nullLogger content

    Assert.Equal(None, host)
    Assert.Equal(None, port)

[<Fact>]
let ``parse config file returns None with wrong port type test`` () =
    let content = $"<config><nohostdefined>somevalue</nohostdefined><port>not uint</port></config>"

    let host, port = read.parseConfigFile mock.nullLogger content

    Assert.Equal(None, host)
    Assert.Equal(Some 0u, port)

[<Fact>]
let ``not existing config file works throws exception test`` () =
    let target = target "no reg key" "no value" "C:\notexisting"

    let ex = Record.Exception(fun () -> box (target.Get()))

    Assert.IsType<IOException>(ex)

[<Fact>]
let ``Configuration does not exists returns false test`` () =

    let target = target "no reg key" "no value" "C:\no fallback"

    let doesExist = target.ConfigurationExists()

    Assert.False(doesExist)

[<Fact>]
let ``Configuration contains wrong content returns false test`` () =
    let fallback = "C:/tmp/providertest.xml"
    files.createWrongConfigFile fallback
    let target = target "no reg key" "no value" fallback

    let doesExist = target.ConfigurationExists()

    Assert.False(doesExist)

    files.deleteConfigFile fallback

[<Fact>]
let ``Configuration exists returns true test`` () =
    let fallback = "C:/tmp/providertest.xml"
    files.createConfigFile fallback "the host" 9828
    let target = target "no reg key" "no value" fallback

    let doesExist = target.ConfigurationExists()

    Assert.True(doesExist)

    files.deleteConfigFile fallback

[<Fact>]
let ``host but wrong port returns false test`` () =
    let fallback = "C:/tmp/providertest.xml"
    files.createConfigFile fallback "the host" "no uint port"
    let target = target "no reg key" "no value" fallback

    let doesExist = target.ConfigurationExists()

    Assert.False(doesExist)

    files.deleteConfigFile fallback

[<Fact>]
let ``host is read from fallback path test`` () =
    let fallback = "C:/tmp/providertest.xml"
    let host = "The Host"
    files.createConfigFile fallback host 9181
    let target = target "no reg key" "no value" fallback

    let config = target.Get()

    Assert.Equal(host, config.HostsAndPorts |> Seq.head |> fun x -> x.Host )

    files.deleteConfigFile fallback
