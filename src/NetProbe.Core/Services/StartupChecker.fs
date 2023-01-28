namespace NetProbe.Core.Services

open NetProbe.Core.Interfaces

type StartupChecker ()=
    interface IStartupChecker with
        member _.CanStart  =
            true
