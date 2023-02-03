namespace NetProbe.Core.Interfaces

open NetProbe.Core.ValueObjects

type IProbe =
    abstract member Test: config: ProbeConfiguration -> printAlways: bool -> unit
