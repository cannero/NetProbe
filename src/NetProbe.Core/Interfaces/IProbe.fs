namespace NetProbe.Core.Interfaces

open NetProbe.Core.ValueObjects

type IProbe =
    /// Returns true if the Test was successful
    abstract member Test: config: ProbeConfiguration -> logInfo: bool -> bool
