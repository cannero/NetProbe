namespace NetProbe.Core.Services

open System.IO
open Microsoft.Extensions.Logging
open NetProbe.Core.Interfaces

type StartupChecker (logger:ILogger<StartupChecker>, registryReader:IRegistryReader)=
    interface IStartupChecker with
        member _.CanStart regKey regValueName fallback =
            let filepath =
                match registryReader.GetLocalKey regKey regValueName with
                | None ->
                    logger.LogWarning("Reg key '{key}' and value '{value}' not found, " +
                                      "using '{fallback}'" , regKey, regValueName, fallback)
                    fallback
                | Some(filepath) -> filepath

            File.Exists(filepath)
