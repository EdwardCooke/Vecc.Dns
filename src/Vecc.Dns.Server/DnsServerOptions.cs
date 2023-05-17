using System.Net;

namespace Vecc.Dns.Server
{
    public class DnsServerOptions
    {
        public IPAddress ListenTCPAddress { get; set; } = IPAddress.Any;
        public int ListenTCPPort { get; set; } = 53;
        public IPAddress ListenUDPAddress { get; set; } = IPAddress.Any;
        public int ListenUDPPort { get; set; } = 53;
    }
}
