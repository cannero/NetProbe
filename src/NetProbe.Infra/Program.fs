open System.Threading
open classtest
//open Pinging
open Looping

let looper = Looper ()
looper.start ()
Thread.Sleep(3000)
looper.stop ()

// let cts = new CancellationTokenSource()
// Async.Start(call.callit (), cts.Token)
// Thread.Sleep(3000)
// cts.Cancel()


//let clt = new Someclass ()
//printfn "%A" (clt.getName ())

//Pinging.runPing "google.de"
//Pinging.runPing "nononon.de"

// expose to c# https://gist.github.com/swlaschin/2d3e75a2ff4a87112c19309c86e0dd41

// read registry
// can start check
// tcp/icmp statistics

// run in background backgroundTask
// https://stackoverflow.com/questions/26706149/f-continuous-loop-in-f
// or actor framework MailboxProcessor
// or timer

// test mysql
// ping server
// get status

// implement interface
// inject logger

// get settings

// export files
