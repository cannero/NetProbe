module Pinging

    open System.Net.NetworkInformation
    open System.Text

    let printPingReply (r:PingReply) =
        printfn "%A %A" r.Status r.RoundtripTime

    let runPing (host:string) =
        let ping = new Ping()
        let pingOptions = PingOptions()
        let buffer = Array.create 30 '-' |> Encoding.ASCII.GetBytes

        try
            ping.Send(host, 1000, buffer, pingOptions)
            |> printPingReply
        with
            | :? PingException as ex ->
                printfn "ping failed: %A" ex.Message
                if ex.InnerException <> null then
                    printfn "%A" ex.InnerException.Message
            
            | ex -> printfn "something else %A" ex

        // let replies =
        //     [|"127.0.0.1" ; "google.de"|]
        //     |> Array.map(fun host -> host, ping.Send(host, 1000, buffer, pingOptions))

        // replies |> Array.iter(fun (h, r) -> printfn "for %s %A %A" h r.Status r.RoundtripTime)
            

