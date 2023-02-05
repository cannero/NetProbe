module MySqlConnectionProbeTests

open Xunit
open NetProbe.Infra.IO
open LoggerRecorder
open ProbeHelpers

let createTarget =
    createIProbeAndLogger (fun l -> MySqlConnectionProbe l)

[<Fact>]
let ``open connection to not exitsting host fails test`` () =
    let target, recorder = createTarget
    let result = runTest target "unknown host"
    let logConnectionFailed = loggerContains recorder "failed"

    Assert.True(logConnectionFailed, allLogs recorder)
    Assert.False result
    
