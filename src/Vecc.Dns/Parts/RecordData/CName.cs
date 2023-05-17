namespace Vecc.Dns.Parts.RecordData
{
    public class CName : RData
    {
        public override ResourceRecordTypes Type => ResourceRecordTypes.CNAME;

        public override Classes Class { get; set; } = Classes.Any;

        public override bool HasClass => true;

        public override bool HasTTL => true;

        public Name Target { get; set; } = new Name();

        public override bool Read(MemoryStream stream, ILogger logger)
        {
            if (!TryReadUShort(stream, out var length))
            {
                logger.LogWarning("Unable to read length in {@class}", nameof(CName));
                return false;
            }
            var currentPosition = stream.Position;

            if (!Target.Read(stream, logger))
            {
                logger.LogWarning("Unable to read target in {@class}", nameof(CName));
                return false;
            }

            if (stream.Position != currentPosition + length)
            {
                logger.LogWarning("New position not in the expected spot, expected {@expected}, actual {@} in {@class}", currentPosition + length, stream.Position, nameof(CName));
                return false;
            }

            logger.LogVerbose("{@class} read", nameof(CName));
            return true;
        }

        public override bool Write(MemoryStream stream, ILogger logger)
        {
            Write(stream, (ushort)(Target.Length + 1));

            if (!Target.Write(stream, logger))
            {
                logger.LogWarning("Unable to write target in {@class}", nameof(CName));
                return false;
            }

            logger.LogVerbose("{@class} written", nameof(CName));
            return true;
        }

        public override string ToShortString() => ToString();

        public override string ToString() => Target.ToString();
    }
}
