namespace NetProbe.Infra.IO

open Microsoft.Extensions.Logging
open NetProbe.Core.Interfaces
open NetProbe.Core.ValueObjects

module private connecting =
    open MySql.Data.MySqlClient
    
    let connect (logger: ILogger<'a>) (hostAndPort: HostAndPort) user pwd logInfo =
        let connString =
            sprintf
                "server=%s;port=%u;uid=%s;pwd=%s"
                hostAndPort.Host
                hostAndPort.Port
                user
                pwd
        
        try
            use reader = MySqlHelper.ExecuteReader(connString, "SHOW DATABASES;")
            if logInfo then
                let mutable count = 0
                while reader.Read() do
                    count <- count + 1
                logger.LogInformation("Database count {count}", count)
                true
            else
                reader.HasRows && reader.FieldCount > 0
        with
            | :? MySqlException as ex ->
                logger.LogError("MySql Connection failed: {message}", ex.Message)
                false
            | ex ->
                logger.LogError(ex, "mysql connection opened not with MySqlException")
                false

/// This probe runs a simple connection test with a 'SHOW DATABASES' command against the database.
type MySqlConnectionProbe (logger : ILogger<MySqlConnectionProbe>) =
    interface IProbe with
        member _.Test config logInfo =
            config.HostsAndPorts
            |> Seq.map (fun hostPort ->
                        connecting.connect
                            logger
                            hostPort
                            config.MySqlUser
                            config.MySqlPassword
                            logInfo)
            |> Seq.exists ((=) false)
            |> not
