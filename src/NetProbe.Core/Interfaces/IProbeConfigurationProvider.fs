namespace NetProbe.Core.Interfaces

open NetProbe.Core.ValueObjects

type IProbeConfigurationProvider =
    abstract member ConfigurationExists: unit -> bool
    /// Check first if configuration can be retrieved by calling ConfigurationExists
    abstract member Get: unit -> ProbeConfiguration
