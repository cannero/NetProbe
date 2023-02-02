namespace NetProbe.Core.Interfaces

open NetProbe.Core.ValueObjects

type IStartupChecker =
    abstract member CanStart : unit -> bool
