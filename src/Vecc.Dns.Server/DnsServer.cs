//#define TestPacket
using System.Net;
using System.Net.Sockets;

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
            await ListenUDPAsync(cancellationToken);
        }

        public virtual async Task ProcessPacketAsync(UdpReceiveResult udpPacket, Socket udpClient) =>
            await ProcessPacketAsync(udpPacket.Buffer, async (buffer) => await udpClient.SendToAsync(buffer, udpPacket.RemoteEndPoint));

        protected virtual async Task ListenUDPAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting UDP server on {udpaddress}:{udpport}", _options.ListenUDPAddress, _options.ListenUDPPort);
            using var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);

            _logger.LogInformation("Binding to UDP:{@address} {@port}", _options.ListenUDPAddress, _options.ListenUDPPort);
            socket.Bind(new IPEndPoint(_options.ListenUDPAddress, _options.ListenUDPPort));
            _logger.LogInformation("Bound to UDP port");

            _logger.LogVerbose("Allocating UDP receive buffer");
            var bufferArray = GC.AllocateArray<byte>(length: 65527, pinned: true);
            var buffer = bufferArray.AsMemory();

            _logger.LogVerbose("Creating UDP IP Address factory");
            var ipaddressFactory = new IPEndPoint(IPAddress.Any, 53);

            _logger.LogVerbose("Creating UDP SocketAddress");
            var receivedAddress = new SocketAddress(socket.AddressFamily);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        using var scope = _logger.CreateScope("PacketId:{@packetId}", Guid.NewGuid().ToString());

                        _logger.LogDebug("Waiting for UDP DNS packet");
                        var receivedCount = await socket.ReceiveFromAsync(buffer, SocketFlags.None, receivedAddress, cancellationToken);
                        _logger.LogDebug("Packet received: Length={length}", receivedCount);

                        var receivedBytes = new byte[receivedCount];
                        _logger.LogVerbose("Created array");

                        Array.Copy(buffer.ToArray(), receivedBytes, receivedCount);
                        _logger.LogVerbose("Copied packet bytes");

                        var endpoint = ipaddressFactory.Create(receivedAddress);
                        _logger.LogVerbose("Created endpoint");

                        var packet = new UdpReceiveResult(receivedBytes, (IPEndPoint)endpoint);
                        _logger.LogVerbose("Created receive result");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(async () =>
                        {
                            try
                            {
                                _logger.LogDebug("Processing packet");
                                await ProcessPacketAsync(packet, socket);
                                _logger.LogDebug("Packet processed");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error processing packet");
                            }
                        }, cancellationToken).ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        _logger.LogVerbose("Task queued");
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
