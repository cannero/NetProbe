namespace NetProbe.Core.Services

open System.Threading
open Microsoft.Extensions.Logging
open NetProbe.Core.Interfaces

module looping =
    let looptime = 500

    /// The cancellation should be checked during each nested async call.
    /// This is described here https://fsharpforfunandprofit.com/posts/concurrency-async-and-parallel/,
    /// but seems not to work always https://stackoverflow.com/questions/21188452/when-does-f-async-check-its-cancellationtoken?noredirect=1&lq=1.
    /// Could reproduce the problem in fsi.
    /// Could be checked after the sleep also with a nop
    /// https://stackoverflow.com/questions/18676657/can-i-explicitly-check-for-cancellation-terminate-async-computation
    let rec loop (probes: IProbe seq) config loopCount = async {
        for probe in probes do
            probe.Test config (loopCount % 2000 = 0) |> ignore
        do! Async.Sleep looptime
        return! loop probes config (if loopCount > 100_000 then 0 else loopCount + 1)}

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
           
