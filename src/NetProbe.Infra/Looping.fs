namespace Looping

open System.Threading
// https://stackoverflow.com/questions/26706149/f-continuous-loop-in-f
// wrap start with backgroundTask?
// async must be used, task is not tail recursive

module loopf =
    let rec loop () = async {
        printfn "in async"
        do! Async.Sleep 500
        return! loop () }


type Looper() =
    let mutable _cts = None
    member _.start () = //async {
            let cts = new CancellationTokenSource()
            _cts <- Some(cts)
            //loopf.loop ()
            printfn "starting"
            Async.Start(loopf.loop (), cts.Token)
            //}

    member _.stop () =
        match _cts with
            | None -> ()
            | Some(token) -> token.Cancel()
        _cts <- None
        printfn "stop"

// module call = 
//     let callit () = async {
//         let looper = Looper ()
//         try 
//             do! looper.start () 
//         finally
//             printfn "That's it!" }
    
    
