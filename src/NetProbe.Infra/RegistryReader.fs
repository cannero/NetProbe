module RegistryReader

open Microsoft.Win32;

[<Literal>]
let ValueNotFound = "-- value not found --"

let openLocal key valuename =
    let rkLocalMachine = Registry.LocalMachine;
    let rkey = rkLocalMachine.OpenSubKey(key);
    let value =
        match rkey with
        | null -> None
        | _ -> let temp = Some(string (rkey.GetValue(valuename, ValueNotFound)))
               rkey.Close();
               temp
    rkLocalMachine.Close();
    value
