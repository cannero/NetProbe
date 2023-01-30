namespace NetProbe.Core.Services

open System.Threading
open Microsoft.Extensions.Logging
open NetProbe.Core.Interfaces

module looping =
    let looptime = 500

    let rec loop probes = async {
        List.iter (fun (probe : IProbe) -> probe.test false) probes
        do! Async.Sleep looptime
        return! loop probes}

type AvailabilityService (logger: ILogger<AvailabilityService>)=
    let mutable cts = None
    let mutable probes = []

    member this.start () =
        this.stop ()
        let newCts = new CancellationTokenSource()
        cts <- Some(newCts)
        Async.Start (looping.loop probes, newCts.Token)

    member _.stop () =
        match cts with
        | None -> ()
        | Some(token) -> token.Cancel()
        cts <- None
    
    interface IAvailabilityService with
        member _.AddProbe probe =
            probes <- probe :: probes
            ()

        member this.Start () =
            this.start ()

        member this.Stop () =
            this.stop ()
           
