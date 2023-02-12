namespace NetProbe.Core.Interfaces

open System

/// Send messages between instances.
type IPiper =
    abstract member Start: callback: Action -> unit
    abstract member Stop: unit -> unit
    abstract member SendAlreadyRunning: unit -> unit
