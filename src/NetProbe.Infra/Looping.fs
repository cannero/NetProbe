namespace Looping

open System.Threading
// https://stackoverflow.com/questions/26706149/f-continuous-loop-in-f
// wrap start with backgroundTask?
// async must be used, task is not tail recursive

module loopf =
    let looptime = 500

    let rec loop (fn) = async {
        fn()
        do! Async.Sleep looptime
        return! loop (fn) }


type Looper() =
    let mutable _cts = None
    member this.start (fn) = //async {
            this.stop()
            let cts = new CancellationTokenSource()
            _cts <- Some(cts)
            //loopf.loop ()
            Async.Start(loopf.loop (fn), cts.Token)
            //}

    member _.stop () =
        match _cts with
            | None -> ()
            | Some(token) -> token.Cancel()
        _cts <- None

// module call = 
//     let callit () = async {
//         let looper = Looper ()
//         try 
//             do! looper.start () 
//         finally
//             printfn "That's it!" }
    
    
