namespace NetProbe.Core.Interfaces

type IAvailabilityService =
    abstract member AddProbe : IProbe -> unit
    abstract member Start : unit -> unit
    abstract member Stop : unit -> unit
