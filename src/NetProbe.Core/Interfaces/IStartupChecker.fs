namespace NetProbe.Core.Interfaces

type IStartupChecker =
    abstract member CanStart : unit -> bool
