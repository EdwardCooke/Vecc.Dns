// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
await dnsServer.ExecuteAsync(CancellationToken.None);
