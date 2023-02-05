module ProbeHelpers

open NetProbe.Core.Interfaces
open NetProbe.Core.ValueObjects
open LoggerRecorder

let createIProbeAndLogger<'p> createType =
    let recorder = LoggerRecorder<'p>()
    createType(recorder) :> IProbe, recorder

let runTest (target: IProbe) host =
    let config = { HostsAndPorts = [{Host = host; Port = 3306u}];
                   MySqlUser = "user"; MySqlPassword = "pwd"; }
    target.Test config true
