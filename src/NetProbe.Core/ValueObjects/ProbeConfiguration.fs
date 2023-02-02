namespace NetProbe.Core.ValueObjects

type ProbeConfiguration = {
     Hosts: string seq
     MySqlUser: string
     MySqlPassword: string
}
