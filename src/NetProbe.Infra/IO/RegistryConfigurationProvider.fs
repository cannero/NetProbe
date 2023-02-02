namespace NetProbe.Infra.IO

open System.IO
open System.Xml.Linq
open Microsoft.Extensions.Logging
open NetProbe.Core.Interfaces

module read =
    let filepath (logger: ILogger<'a>) (registryReader: IRegistryReader) regConfig =
        match registryReader.GetLocalKey regConfig with
        | None ->
            logger.LogWarning("Reg key '{key}' and value '{value}' not found, " +
                              "using fallback '{fallback}'" ,
                              regConfig.Key,
                              regConfig.ValueName,
                              regConfig.FallbackPath)
            regConfig.FallbackPath
        | Some(filepath) -> filepath

    let parseConfigFile (logger: ILogger<'a>) content =
        let doc = XDocument.Parse content
        let outer = doc.Element("config")
        match outer with
        | null ->
            logger.LogError("no config element found in '{content}'", content)
            None
        | _ ->
            let hostele = outer.Element("host")
            match hostele with
            | null ->
                logger.LogError("no host element found in '{content}'", content)
                None
            | _ ->
                let host = hostele.Value.Trim()
                logger.LogInformation("using host '{host}'", host)
                Some host

    let getHost logger path =
        let content = File.ReadAllText path
        parseConfigFile logger content

type RegistryConfigurationProvider (logger, regConfig, registryReader) =
    interface IProbeConfigurationProvider with
        member _.ConfigurationExists () =
            let path = read.filepath logger registryReader regConfig
            match File.Exists path with
            | false -> false
            | true ->
                match read.getHost logger path with
                | None -> false
                | Some _ -> true

        member _.Get () =
            let path = read.filepath logger registryReader regConfig
            let host = read.getHost logger path
            { Host = host.Value; MySqlUser = "root"; MySqlPassword = ""; }


