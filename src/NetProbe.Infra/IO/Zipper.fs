namespace NetProbe.Infra.IO


open Microsoft.Extensions.Logging
open NetProbe.Core.Interfaces

module filehandling =
    open System.IO
    open System.IO.Compression

    let checkForExisting dataPath outputPath =
        if File.Exists outputPath then
            File.Delete outputPath
        dataPath, outputPath

    let createZip dataPath outputPath =
        ZipFile.CreateFromDirectory(dataPath, outputPath)

type Zipper (logger : ILogger<Zipper>, dataPath) =
    interface IZipper with
        member this.ZipIt outputPath =
            try
                (dataPath, outputPath)
                ||> filehandling.checkForExisting
                ||> filehandling.createZip
                true
            with
                | ex ->
                    logger.LogError(ex, "ZipIt: zip file could not be created")
                    false
