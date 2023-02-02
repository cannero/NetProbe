namespace NetProbe.Core.Interfaces

type IProbe =
    abstract member Test: printAlways: bool -> unit
