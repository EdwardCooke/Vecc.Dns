// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using Vecc.Dns.Example;
using Vecc.Dns.Server;

var serviceCollection = new ServiceCollection();
serviceCollection.AddLogging((log) =>
{
    log.AddConsole();
});
serviceCollection.AddSingleton<Vecc.Dns.ILogger, LoggingExtensionsLogger>();
serviceCollection.AddSingleton<DnsServer>();
serviceCollection.AddSingleton<IDnsResolver, TestDnsResolver>();
serviceCollection.AddSingleton(new DnsServerOptions
{
    ListenTCPPort = 1053,
    ListenUDPPort = 1053
});

var serviceProvider = serviceCollection.BuildServiceProvider();

var dnsServer = serviceProvider.GetRequiredService<DnsServer>();
Task server = dnsServer.ExecuteAsync(CancellationToken.None);

//Send test data - this is an example request from a Microsoft DNS server, requesting a domain and sending an opt additional record for dnssec
var hexString = "92CE01000001000000000001046172676F07706F63312D6871046B756265056D746E616D036F726700000100010000290FA0000080000000";
var bytes = new byte[hexString.Length / 2];
for (var i = 0; i < bytes.Length; i++)
{
    bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
}

UdpClient udpClient = new UdpClient(1054);
await udpClient.SendAsync(bytes, "localhost", 1053);
Console.ReadLine();
