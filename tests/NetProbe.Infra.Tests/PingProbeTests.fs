module PingProbeTests

open Xunit
open NetProbe.Core.Interfaces
open NetProbe.Core.ValueObjects
open NetProbe.Infra.IO
open NetProbe.Infra.Tests

let createTarget =
    let recorder = LoggerRecorder<PingProbe> ()
    PingProbe recorder :> IProbe, recorder

let runTest (target: IProbe) host =
    let config = { Hosts = [host]; MySqlUser = "user"; MySqlPassword = "pwd"; }
    target.Test config false

let allLogs (recorder : LoggerRecorder<PingProbe>) =
    recorder.getLogs () |> List.fold (fun t l -> t + "; " + l) ""

[<Fact>]
let ``ping localhost is successful test`` () =
    let target, recorder = createTarget
    runTest target "127.0.0.1"
    let pingSuccessful = recorder.getLogs () |> List.exists (fun l -> l.Contains("Success"))
    Assert.True(pingSuccessful, allLogs recorder)

[<Fact>]
let ``ping not exitsting host logs error test`` () =
    let target, recorder = createTarget
    runTest target "UsdfjThisDoesNotExist.com"
    let pingFailed = recorder.getLogs () |> List.exists (fun l -> l.Contains("No such host is known"))
    Assert.True(pingFailed, allLogs recorder)
