namespace NetProbe.Infra.Dummys

open NetProbe.Core.Interfaces

type FailingZipper () =
    interface IZipper with
        member _.ZipIt _outputPath =
            false

