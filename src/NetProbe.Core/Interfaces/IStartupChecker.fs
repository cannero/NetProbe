namespace NetProbe.Core.Interfaces

type IStartupChecker =
    abstract member CanStart : regKey:string -> regValue:string -> fallbackPath:string -> bool
