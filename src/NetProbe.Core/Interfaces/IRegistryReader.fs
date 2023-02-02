namespace NetProbe.Core.Interfaces

open NetProbe.Core.ValueObjects

type IRegistryReader =
    abstract member GetLocalKey : config: RegistryConfiguration -> string option
