using System.Net;

namespace Vecc.Dns.Parts.RecordData
{
    public class A : RData
    {
        public IPAddress? IPAddress { get; set; }

        public override ResourceRecordTypes Type => ResourceRecordTypes.A;

        public override Classes Class { get; set; } = Classes.Any;

        public override bool HasClass => true;

        public override bool HasTTL => true;

        public override bool Read(MemoryStream stream, ILogger logger)
        {
            if (!TryReadUShort(stream, out var length))
            {
                logger.LogWarning("Unable to read length in {@class}", nameof(A));
                return false;
            }

            if (length != 4)
            {
                logger.LogWarning("Length != 4 in {@class} actual {@length}", nameof(A), length);
                return false;
            }

            var currentPosition = stream.Position;

            var buffer = new byte[4];
            var bytesRead = stream.Read(buffer);
            if (bytesRead != 4)
            {
                logger.LogWarning("Bytes read != 4 in {@class} actual {@bytesread}", nameof(A), bytesRead);
                return false;
            }

            IPAddress = new IPAddress(buffer);
            if (stream.Position != currentPosition + 4)
            {
                logger.LogWarning("New stream position not expected in {@class}. expected {@expected}, actual {@actual}", nameof(A), currentPosition + 4, stream.Position);
                return false;
            }

            logger.LogVerbose("{@class} read. address {@address}", IPAddress.ToString());
            return true;
        }

        public override bool Write(MemoryStream stream, ILogger logger)
        {
            if (IPAddress == null)
            {
                logger.LogWarning("IPAddress is required in {@class}", nameof(A));
                return false;
            }

            var buffer = IPAddress.GetAddressBytes();
            Write(stream, (ushort)buffer.Length);
            stream.Write(buffer);

            logger.LogVerbose("{@class} written", nameof(A));
            return true;
        }

        public override string ToShortString() => ToString();

        public override string ToString() => IPAddress?.ToString() ?? "NULL";
    }
}
