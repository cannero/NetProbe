namespace NetProbe.Infra.IO

open Microsoft.Win32
open NetProbe.Core.Interfaces

type RegistryReader () =
    interface IRegistryReader with
        member _.GetLocalKey key valuename  =
            let rkLocalMachine = Registry.LocalMachine;
            let rkey = rkLocalMachine.OpenSubKey(key);
            let value =
                match rkey with
                | null -> None
                | _ -> let temp =
                           match
                               rkey.GetValue(valuename)
                           with
                           | null -> None
                           | value -> Some(string value)
                       rkey.Close();
                       temp
            rkLocalMachine.Close();
            value
