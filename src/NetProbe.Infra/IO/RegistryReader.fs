namespace NetProbe.Infra.IO

open Microsoft.Win32
open NetProbe.Core.Interfaces

type RegistryReader () =
    interface IRegistryReader with
        member _.GetLocalKey regConfig  =
            let rkLocalMachine = Registry.LocalMachine;
            let rkey = rkLocalMachine.OpenSubKey(regConfig.Key);
            let value =
                match rkey with
                | null -> None
                | _ -> let temp =
                           match
                               rkey.GetValue(regConfig.ValueName)
                           with
                           | null -> None
                           | value -> Some(string value)
                       rkey.Close();
                       temp
            rkLocalMachine.Close();
            value
