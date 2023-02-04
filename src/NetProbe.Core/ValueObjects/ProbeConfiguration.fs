namespace NetProbe.Core.ValueObjects

type ProbeConfiguration = {
     HostsAndPorts: HostAndPort seq
     MySqlUser: string
     MySqlPassword: string
}
