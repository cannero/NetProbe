namespace NetProbe.Core.Services

open System.Threading
open Microsoft.Extensions.Logging
open NetProbe.Core.Interfaces

module looping =
    let looptime = 500

    let rec loop (probes: IProbe seq) config loopCount = async {
        for probe in probes do
            probe.Test config (loopCount % 2000 = 0) |> ignore
        do! Async.Sleep looptime
        let newLoopCount = if loopCount > 100_000 then
                               0
                           else
                               loopCount + 1
        return! loop probes config (newLoopCount)}

type AvailabilityService
    (
        logger: ILogger<AvailabilityService>,
        configProvider : IProbeConfigurationProvider
    ) =
    let mutable cts = None
    let mutable probes = []

    member this.start () =
        this.stop ()
        let newCts = new CancellationTokenSource()
        cts <- Some(newCts)
        let config = configProvider.Get()
        Async.Start (looping.loop probes config 0, newCts.Token)

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
           
