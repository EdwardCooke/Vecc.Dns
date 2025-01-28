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

        public ushort Version { get; set; }

        public byte[] RData { get; set; } = Array.Empty<byte>();

        public bool DNSSecSupported { get; set; }

        public ushort Z { get; set; }

        public override bool Read(MemoryStream stream, ILogger logger)
        {
            if (!TryReadUShort(stream, out var value))
            {
                logger.LogWarning("Unable to read payload size in {@class}", nameof(Opt));
                return false;
            }
            RequestorsPayloadSize = value;

            if (!TryReadUInt(stream, out var ttlField))
            {
                logger.LogWarning("Unable to read TTL Field in {@class}", nameof(Opt));
                return false;
            }

            RCode =   (ushort)((ttlField & 0xFF000000) >> 24);
            Version = (ushort)((ttlField & 0x00FF0000) >> 16);
            DNSSecSupported =  (ttlField & 0x00008000) == 0x8000;
            Z =        (ushort)(ttlField & 0x00007FFF);


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

            var ttlField = (uint)(RCode << 24);
            ttlField |= (uint)(Version << 16);
            ttlField |= DNSSecSupported ? (uint)0x8000 : 0;
            ttlField |= Z;

            Write(stream, ttlField);
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
