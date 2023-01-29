namespace NetProbe.Core.Services

open System.IO
open NetProbe.Core.Interfaces

type StartupChecker (registryReader:IRegistryReader)=
    interface IStartupChecker with
        member _.CanStart regKey regValueName fallback =
            let filepath =
                match registryReader.GetLocalKey regKey regValueName with
                | None -> fallback
                | Some(filepath) -> filepath

            File.Exists(filepath)
