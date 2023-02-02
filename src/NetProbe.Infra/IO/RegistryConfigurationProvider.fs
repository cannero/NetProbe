namespace NetProbe.Infra.IO

open System.IO
open System.Xml.Linq
open Microsoft.Extensions.Logging
open NetProbe.Core.Interfaces

module read =
    let filepath (logger: ILogger<'a>) (registryReader: IRegistryReader) regKey regValueName fallback =
        match registryReader.GetLocalKey regKey regValueName with
        | None ->
            logger.LogWarning("Reg key '{key}' and value '{value}' not found, " +
                              "using '{fallback}'" , regKey, regValueName, fallback)
            fallback
        | Some(filepath) -> filepath

    let parseConfigFile (logger: ILogger<'a>) content =
        let doc = XDocument.Parse content
        doc.Element("config").Element("host").Value
        


type RegistryConfigurationProvider (logger, regKey, regValueName, fallback, registryReader) =
    interface IProbeConfigurationProvider with
        member _.Get () =
            let path = read.filepath logger registryReader regKey regValueName fallback
            let content = File.ReadAllText path
            let host = read.parseConfigFile logger content
            { Host = "127.0.0.1"; MySqlUser = "root"; MySqlPassword = ""; }
           //let path = read.filepath logger registryReader regKey regValueName fallback

