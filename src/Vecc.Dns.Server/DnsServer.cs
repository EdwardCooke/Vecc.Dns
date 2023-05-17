//#define TestPacket
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Vecc.Dns.Parts;
using Vecc.Dns.Parts.RecordData;

namespace Vecc.Dns.Server
{
    public class DnsServer : IDnsServer
    {
        private readonly IDnsResolver _resolver;
        private readonly DnsServerOptions _options;
        private readonly ILogger _logger;

        public DnsServer(IDnsResolver resolver, DnsServerOptions options, ILogger logger)
        {
            _resolver = resolver;
            _options = options;
            _logger = logger;
        }

        public virtual async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting servers");
            var udpTask = Task.Run(async () => await ListenUDPAsync(cancellationToken));
            //var tcpTask = Task.Run(ListenTCPAsync);
            //var tcpClient = new TcpClient(new IPEndPoint(_options.ListenTCPAddress, _options.ListenTCPPort));
            await Task.WhenAll(udpTask);
        }

        public virtual async Task ProcessPacketAsync(UdpReceiveResult udpPacket, UdpClient udpClient) =>
            await ProcessPacketAsync(udpPacket.Buffer, async (buffer) => await udpClient.SendAsync(buffer, buffer.Length, udpPacket.RemoteEndPoint));

        protected virtual async Task ListenUDPAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting UDP server on {udpaddress}:{udpport}", _options.ListenUDPAddress, _options.ListenUDPPort);
            var client = new UdpClient(new IPEndPoint(_options.ListenUDPAddress, _options.ListenUDPPort));
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var packet = await client.ReceiveAsync(cancellationToken);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(async () => await ProcessPacketAsync(packet, client), cancellationToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Cleanly shutting down UDP server");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled exception processing request");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogFatal(ex, $"Unhandled exception in {nameof(ListenUDPAsync)} method");
            }
        }

        protected async Task ProcessPacketAsync(byte[] buffer, Func<byte[], Task> sender)
        {
            var packet = new Packet();
            Packet? answer = default;
            var initialized = false;

            try
            {
                var memoryStream = new MemoryStream(buffer);
                if (!packet.Read(memoryStream, _logger))
                {
                    _logger.LogWarning("Unable to read packet: {@buffer}", buffer);
                    return;
                }
                initialized = true;

                try
                {
                    answer = await _resolver.ResolveAsync(packet);
                    if (answer == null)
                    {
                        answer = await _resolver.GetEmptyResponseAsync(packet);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unhandled exception resolving answer: {@packet}", packet);
                }

                if (answer != null)
                {
                    using var responseStream = new MemoryStream();
                    answer.Write(responseStream, _logger);

                    var responseBuffer = responseStream.ToArray();
                    try
                    {
                        await sender(responseBuffer);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending udp packet to requestor {@request} {@answer}", packet, answer);
                    }
                    _logger.LogInformation("{@request} -> {@answer}", packet.ToShortString(), answer.ToShortString());
                }
                else
                {
                    _logger.LogError("Answer was null {@request}", packet);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception processing packet, {@bytes} {@packetread} {@packet} {@answer}", buffer, initialized, packet, answer);
            }
        }
    }
}
