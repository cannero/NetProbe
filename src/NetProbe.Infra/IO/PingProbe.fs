namespace NetProbe.Infra.IO

open Microsoft.Extensions.Logging
open NetProbe.Core.Interfaces

module private pinging =

    open System.Net.NetworkInformation
    open System.Text

    let printPingReply (logger : ILogger) logInfo (r : PingReply) =
        if logInfo then
            logger.LogInformation("status: {status}, roundtrip time {time}", r.Status, r.RoundtripTime)

    let runPing (logger : ILogger) (host : string) logInfo =
        let ping = new Ping()
        let pingOptions = PingOptions()
        let buffer = Array.create 30 '-' |> Encoding.ASCII.GetBytes

        try
            ping.Send(host, 1000, buffer, pingOptions)
            |> printPingReply logger logInfo
            true
        with
            | :? PingException as ex ->
                logger.LogError("Ping failed: {message}", ex.Message)
                if ex.InnerException <> null then
                    logger.LogError("{message}", ex.InnerException.Message)
                false
            | ex ->
                logger.LogError(ex, "pinging failed not with PingException")
                false


type PingProbe (logger : ILogger<PingProbe>) =
    interface IProbe with
        member _.Test config logInfo =
            config.HostsAndPorts
            |> Seq.map (fun hostPort -> hostPort.Host)
            |> Seq.map (fun h -> pinging.runPing logger h logInfo)
            |> Seq.exists ((=) false)
            |> not


        // let replies =
        //     [|"127.0.0.1" ; "google.de"|]
        //     |> Array.map(fun host -> host, ping.Send(host, 1000, buffer, pingOptions))

        // replies |> Array.iter(fun (h, r) -> printfn "for %s %A %A" h r.Status r.RoundtripTime)
