namespace NetProbe.Core.Interfaces

type IZipper =
    /// Returns true if zip file could be created
    abstract member ZipIt: outputPath: string -> bool
