module ZipperTests

open System.IO
open Microsoft.Extensions.Logging.Abstractions
open Xunit
open NetProbe.Core.Interfaces
open NetProbe.Infra.IO

let testDir = "c:/tmp/zippertestdirectory"
let sourceDir = sprintf "%s/thelog" testDir
let outputPath = sprintf "%s/thefile.zip" testDir

let createTarget () =
    Zipper(NullLogger<Zipper>(), sourceDir) :> IZipper

module files =
    let createTestDirectoryWithLogs () =
        //if not (Directory.Exists sourceDir) then
        Directory.CreateDirectory sourceDir |> ignore
        File.WriteAllText(sourceDir + "/file1", "srjoiwjnoi\newioru\nsiweoioe")
        File.WriteAllText(sourceDir + "/file2", "woieeioww\n\noweirunsdf\nei")

    let deleteTestDirectory () =
        Directory.Delete(testDir, true)

    let createDummyZipFile () =
        File.WriteAllText(outputPath, "sdkljf\nwniweo9r")

    let assertZipFile path =
        let buffer = File.ReadAllBytes path
        Assert.Equal(0x50uy, buffer[0])
        Assert.Equal(0x4buy, buffer[1])
        Assert.Equal(0x03uy, buffer[2])
        Assert.Equal(0x04uy, buffer[3])

[<Fact>]
let ``new file can be created test`` () =
    files.createTestDirectoryWithLogs()
    let target = createTarget()
    let result = target.ZipIt outputPath
    Assert.True result
    Assert.True(File.Exists outputPath)
    files.deleteTestDirectory ()

[<Fact>]
let ``zip file already exists, new file can be created test`` () =
    files.createTestDirectoryWithLogs()
    files.createDummyZipFile()
    let target = createTarget()
    let result = target.ZipIt outputPath
    Assert.True result
    files.assertZipFile outputPath
    files.deleteTestDirectory ()
