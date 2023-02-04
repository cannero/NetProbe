namespace NetProbe.Infra.IO
// implicit 'string' to type 'XName'
#nowarn "3391"

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
        let getEle name (parent: XContainer)  =
            let ele = parent.Element name
            if (isNull ele) then
                logger.LogError("no '{name}' element found in '{content}'", name, content)
                None
            else
                Some ele

        XDocument.Parse content
        |> getEle "config"
        |> Option.map (getEle "host")
        |> Option.flatten
        |> Option.map (fun ele -> ele.Value.Trim())

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
            { Hosts = [host.Value]; MySqlUser = "root"; MySqlPassword = ""; }


