module PingProbeTests

open Xunit
open NetProbe.Core.Interfaces
open NetProbe.Infra.IO
open NetProbe.Infra.Tests

let createTarget host =
    let recorder = LoggerRecorder<PingProbe> ()
    (PingProbe (recorder, [host]) :> IProbe, recorder)

let allLogs (recorder : LoggerRecorder<PingProbe>) =
    recorder.getLogs () |> List.fold (fun t l -> t + "; " + l) ""

[<Fact>]
let ``ping localhost is successful test`` () =
    let target, recorder = createTarget "127.0.0.1"
    target.Test false
    let pingSuccessful = recorder.getLogs () |> List.exists (fun l -> l.Contains("Success"))
    Assert.True(pingSuccessful, allLogs recorder)

[<Fact>]
let ``ping not exitsting host logs error test`` () =
    let target, recorder = createTarget "UsdfjThisDoesNotExist.com"
    target.Test false
    let pingFailed = recorder.getLogs () |> List.exists (fun l -> l.Contains("No such host is known"))
    Assert.True(pingFailed, allLogs recorder)
