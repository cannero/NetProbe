namespace NetProbe.Infra.IO

open System.IO
open System.IO.Pipes
open System.Threading
open Microsoft.Extensions.Logging
open NetProbe.Core.Interfaces

module private piping =
    [<Literal>]
    let PipeName = "NetProbe998877"
    [<Literal>]
    let AlreadyRunningMessage = "[NetProbe]: Already running"

    let startServer (logger: ILogger) (callback: System.Action) =

        let rec loop () = async {
            try
                use pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.In)
                pipeServer.WaitForConnection()
                logger.LogInformation "pipe connected"
                use sr = new StreamReader(pipeServer)

                let message = sr.ReadLine()
                if message = AlreadyRunningMessage then
                    callback.Invoke()
            with
                | _ as e -> logger.LogError(e, "pipe server with error")
            return! loop() }

        loop()

    let sendAlreadyRunning (logger: ILogger) =
        logger.LogInformation "Sending message to pipe server"
        use pipeClient = new NamedPipeClientStream(".", PipeName,
                                                   PipeDirection.Out, PipeOptions.None)
        try
            pipeClient.Connect()
            use sw = new StreamWriter(pipeClient, AutoFlush = true)
            AlreadyRunningMessage |> sw.WriteLine
        with
            | _ as e -> logger.LogError(e, "Pipe Client failed")

type Piper
    (
        logger: ILogger<Piper>
    ) =
    let mutable cts = None
    interface IPiper with
        member _.Start callback =
            if cts.IsSome then
                raise (System.ArgumentException("Pipe already started"))
            let newCts = new CancellationTokenSource()
            cts <- Some(newCts)
            Async.Start (piping.startServer logger callback, newCts.Token)

        member _.Stop () =
            match cts with
            | None -> ()
            | Some(token) ->
                token.Cancel()
                token.Dispose()
                cts <- None

        member _.SendAlreadyRunning () =
            piping.sendAlreadyRunning logger
