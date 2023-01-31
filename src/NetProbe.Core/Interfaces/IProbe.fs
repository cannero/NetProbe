namespace NetProbe.Core.Interfaces

open Microsoft.Extensions.Logging 

type IProbe =
    abstract member Test : printAlways:bool -> unit
