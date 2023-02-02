namespace NetProbe.Core.ValueObjects

[<CLIMutable>]
type RegistryConfiguration = {
     Key: string
     ValueName: string
     FallbackPath: string
}
