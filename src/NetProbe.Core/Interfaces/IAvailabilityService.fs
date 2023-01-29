namespace NetProbe.Core.Interfaces

type IAvailabilityService =
    abstract member Start : unit -> unit
    abstract member Stop : unit -> unit
