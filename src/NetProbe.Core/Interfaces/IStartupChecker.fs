namespace NetProbe.Core.Interfaces

open NetProbe.Core.ValueObjects

type IStartupChecker =
    abstract member CanStart : regKey:string -> regValue:string -> fallbackPath:string -> bool
    abstract member CreateConfig : regKey:string -> regValue:string ->
        fallbackPath:string -> ProbeConfiguration
