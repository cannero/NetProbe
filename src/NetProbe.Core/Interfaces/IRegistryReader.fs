namespace NetProbe.Core.Interfaces

type IRegistryReader =
    abstract member GetLocalKey : key:string -> valuename:string -> string option
