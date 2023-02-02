namespace NetProbe.Core.Services

open System.IO
open Microsoft.Extensions.Logging
open NetProbe.Core.Interfaces
open NetProbe.Core.ValueObjects

module startup =
    let filepath (logger: ILogger<'a>) (registryReader: IRegistryReader) regKey regValueName fallback =
        match registryReader.GetLocalKey regKey regValueName with
        | None ->
            logger.LogWarning("Reg key '{key}' and value '{value}' not found, " +
                              "using '{fallback}'" , regKey, regValueName, fallback)
            fallback
        | Some(filepath) -> filepath

type StartupChecker (logger:ILogger<StartupChecker>, registryReader:IRegistryReader)=
    interface IStartupChecker with
        member _.CanStart regKey regValueName fallback =
            let path = startup.filepath logger registryReader regKey regValueName fallback
            File.Exists(path)

        member _.CreateConfig regKey regValueName fallback =
            { Host = "127.0.0.1"; MySqlUser = "root"; MySqlPassword = ""; }
