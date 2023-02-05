module LoggerRecorder

open System
open Microsoft.Extensions.Logging

type LoggerRecorder<'a>() =
    let mutable logs = []

    member _.getLogs () = logs

    interface ILogger<'a> with
        member _.BeginScope<'s>(state : 's) =
            raise <| NotImplementedException("BeginScope")
        member _.IsEnabled(logLevel) =
            true
        member _.Log<'s>(logLevel, eventId, state : 's, ex, formatter) =
            logs <- (formatter.Invoke(state,ex)) :: logs

let allLogs (recorder: LoggerRecorder<'a>) =
    recorder.getLogs () |> List.fold (fun t l -> t + "; " + l) ""

let loggerContains (recorder: LoggerRecorder<'a>) (text: string) =
    recorder.getLogs () |> List.exists (fun l -> l.Contains(text))
