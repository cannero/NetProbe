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

        let doc = XDocument.Parse content
        let host =
            doc
            |> getEle "config"
            |> Option.map (getEle "host")
            |> Option.flatten
            |> Option.map (fun ele -> ele.Value.Trim())

        let port =
            doc
            |> getEle "config"
            |> Option.map (getEle "port")
            |> Option.flatten
            |> Option.map (fun ele -> ele.Value.Trim())
            |> Option.map (fun p -> try uint p with _ -> 0u)

        host, port

    let getHostAndPort logger path =
        let content = File.ReadAllText path
        parseConfigFile logger content

type RegistryConfigurationProvider (logger, regConfig, registryReader) =
    interface IProbeConfigurationProvider with
        member _.ConfigurationExists () =
            let path = read.filepath logger registryReader regConfig
            match File.Exists path with
            | false -> false
            | true ->
                match read.getHostAndPort logger path with
                | Some _, Some _ -> true
                | _ -> false

        member _.Get () =
            let path = read.filepath logger registryReader regConfig
            let host, port = read.getHostAndPort logger path
            { HostsAndPorts = [{ Host = host.Value; Port = port.Value }]; MySqlUser = "root"; MySqlPassword = ""; }


