namespace Vecc.Dns.Parts.RecordData
{
    public class Opt : RData
    {
        public override ResourceRecordTypes Type => ResourceRecordTypes.OPT;

        public override Classes Class { get; set; } = Classes.Any;

        public override bool HasClass => false;

        public override bool HasTTL => false;

        public ushort RequestorsPayloadSize { get; set; }

        public ushort RCode { get; set; }
        public byte[] RData { get; set; } = Array.Empty<byte>();

        public override bool Read(MemoryStream stream, ILogger logger)
        {
            if (!TryReadUShort(stream, out var value))
            {
                logger.LogWarning("Unable to read payload size in {@class}", nameof(Opt));
                return false;
            }
            RequestorsPayloadSize = value;

            if (!TryReadUShort(stream, out value))
            {
                logger.LogWarning("Unable to read RCode in {@class}", nameof(Opt));
                return false;
            }
            RCode = value;

            if (!TryReadUShort(stream, out var length))
            {
                logger.LogWarning("Unable to read length in {@class}", nameof(Opt));
                return false;
            }

            var buffer = new byte[length];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead != length)
            {
                logger.LogWarning("Bytes read != length in {@class}. Expected {@expected} Actual {@actual}", nameof(Opt), length, bytesRead);
                return false;
            }

            logger.LogVerbose("{@class} read PayloadSize {@payloadsize} {@rcode} {@rdata}", nameof(Opt), RequestorsPayloadSize, RCode, RData);
            return true;
        }

        public override bool Write(MemoryStream stream, ILogger logger)
        {
            Write(stream, RequestorsPayloadSize);
            Write(stream, RCode);
            Write(stream, (uint)RData.Length);
            stream.Write(RData, 0, RData.Length);

            logger.LogVerbose("{@class} written", nameof(Opt));
            return true;
        }
        public override string ToShortString() => $"RCode: {@RCode}  RData: {Convert.ToBase64String(RData)}";

        public override string ToString()
        {
            return $"RequestorsPayloadSize: {RequestorsPayloadSize} RCode: {@RCode}  RData: {Convert.ToBase64String(RData)}";
        }
    }
}
