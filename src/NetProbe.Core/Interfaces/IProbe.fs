namespace NetProbe.Core.Interfaces

open Microsoft.Extensions.Logging 

type IProbe =
    abstract member test : printAlways:bool -> unit
