using System.Net;

namespace Vecc.Dns.Parts.RecordData
{
    public class AAAA : RData
    {
        public IPAddress? IPAddress { get; set; }

        public override ResourceRecordTypes Type => ResourceRecordTypes.AAAA;

        public override Classes Class { get => Classes.Internet; }

        public override bool HasTTL => true;

        public override bool HasClass => true;

        public override bool Read(MemoryStream stream, ILogger logger)
        {
            if (!TryReadUShort(stream, out var length))
            {
                logger.LogWarning("Unable to read length in {@class}", nameof(AAAA));
                return false;
            }

            if (length != 16)
            {
                logger.LogWarning("Length != 16 in {@class} actual {@length}", nameof(AAAA), length);
                return false;
            }

            var currentPosition = stream.Position;

            var buffer = new byte[16];
            var bytesRead = stream.Read(buffer);
            if (bytesRead != 16)
            {
                logger.LogWarning("Bytes read != 16 in {@class} actual {@bytesread}", nameof(AAAA), bytesRead);
                return false;
            }

            IPAddress = new IPAddress(buffer);
            if (stream.Position != currentPosition + 16)
            {
                logger.LogWarning("New stream position not expected in {@class}. expected {@expected}, actual {@actual}", nameof(AAAA), currentPosition + 16, stream.Position);
                return false;
            }

            logger.LogVerbose("{@class} read. address {@address}", IPAddress.ToString());
            return true;
        }

        public override bool Write(MemoryStream stream, ILogger logger)
        {
            if (IPAddress == null)
            {
                logger.LogWarning("IPAddress is required in {@class}", nameof(AAAA));
                return false;
            }

            var buffer = IPAddress.GetAddressBytes();
            Write(stream, (ushort)buffer.Length);
            stream.Write(buffer);

            logger.LogVerbose("{@class} written", nameof(AAAA));
            return true;
        }

        public override string ToShortString() => IPAddress?.ToString() ?? "NULL";
    }
}
