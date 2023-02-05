module PingProbeTests

open Xunit
open NetProbe.Infra.IO
open LoggerRecorder
open ProbeHelpers

let createTarget =
    createIProbeAndLogger (fun l -> PingProbe l)

[<Fact>]
let ``ping localhost is successful test`` () =
    let target, recorder = createTarget
    let result = runTest target "127.0.0.1"
    let logPingSuccessful = loggerContains recorder "Success"

    Assert.True(logPingSuccessful, allLogs recorder)
    Assert.True(result)

[<Fact>]
let ``ping not exitsting host logs error test`` () =
    let target, recorder = createTarget
    let result = runTest target "UsdfjThisDoesNotExist.com"
    let logPingFailed = loggerContains recorder "No such host is known"

    Assert.True(logPingFailed, allLogs recorder)
    Assert.False(result)
