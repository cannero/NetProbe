namespace NetProbe.Core.Interfaces

open NetProbe.Core.ValueObjects

type IProbeConfigurationProvider =
    abstract member Get: unit -> ProbeConfiguration
