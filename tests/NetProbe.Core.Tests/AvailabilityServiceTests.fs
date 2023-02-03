module AvailabilityServiceTests

open System.Threading
open Microsoft.Extensions.Logging.Abstractions
open Xunit
open NetProbe.Core.Interfaces
open NetProbe.Core.Services

module mock =
    let mutable x = 0
    let probe () =
        x <- 0
        { new IProbe with
              member _.Test _config _printAlways =
                  x <- x + 1}

let createTarget () =
    let configProvider() = {
        new IProbeConfigurationProvider with
            member _.ConfigurationExists() =
                true
            member _.Get() =
                { Hosts = ["somehost"]; MySqlUser = "user"; MySqlPassword = "pwd"; }
    }

    let target = AvailabilityService (new NullLogger<AvailabilityService>(), configProvider())
    target :> IAvailabilityService

[<Fact>]
let ``Probe is run multiple times test`` () =
    let target = createTarget ()
    target.AddProbe (mock.probe ())
    target.Start ()
    Thread.Sleep(3 * looping.looptime)
    target.Stop ()
    Assert.True(mock.x >= 3 && mock.x <= 4, (sprintf "x is %i" mock.x))

[<Fact>]
let ``Probes are not run after stop test`` () =
    let target = createTarget ()
    target.AddProbe (mock.probe ())
    target.Start ()
    target.Stop ()
    Thread.Sleep(3 * looping.looptime)
    Assert.True(mock.x <= 1, (sprintf "x is %i" mock.x))

[<Fact>]
let ``Probes are not started multiple times test`` () =
    let target = createTarget ()
    target.AddProbe (mock.probe ())
    target.Start ()
    target.Start ()
    Thread.Sleep(3 * looping.looptime)
    target.Stop ()
    Assert.True(mock.x >= 3 && mock.x <= 4, (sprintf "x is %i" mock.x))
