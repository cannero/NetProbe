namespace NetProbe.Core.Services

open Microsoft.Extensions.Logging
open NetProbe.Core.Interfaces

type StartupChecker (_logger:ILogger<StartupChecker>, configProvider:IProbeConfigurationProvider)=
    interface IStartupChecker with
        member _.CanStart () =
            configProvider.ConfigurationExists()
