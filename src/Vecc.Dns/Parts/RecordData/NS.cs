namespace Vecc.Dns.Parts.RecordData
{
    public class NS : RData
    {
        public override ResourceRecordTypes Type => ResourceRecordTypes.NS;

        public override Classes Class { get; set; } = Classes.Any;

        public override bool HasClass => true;

        public override bool HasTTL => true;

        public Name Target { get; set; } = new Name();

        public override bool Read(MemoryStream stream, ILogger logger)
        {
            if (!TryReadUShort(stream, out var length))
            {
                logger.LogWarning("Unable to read length in {@class}", nameof(NS));
                return false;
            }

            var currentPosition = stream.Position;
            if (!Target.Read(stream, logger))
            {
                logger.LogWarning("Unable to read target in {@class}", nameof(NS));
                return false;
            }


            if (stream.Position != currentPosition + length)
            {
                logger.LogWarning("New position not in the expected spot, expected {@expected}, actual {@} in {@class}", currentPosition + length, stream.Position, nameof(NS));
                return false;
            }

            logger.LogVerbose("{@class} read", nameof(NS));
            return true;
        }

        public override bool Write(MemoryStream stream, ILogger logger)
        {
            var length = 2 //data length
                + Target.Length;

            Write(stream, (ushort)length);

            if (!Target.Write(stream, logger))
            {
                logger.LogWarning("Unable to write target in {@class}", nameof(NS));
                return false;
            }

            logger.LogVerbose("{@class} written", nameof(NS));
            return true;
        }

        public override string ToShortString() => ToString();

        public override string ToString() => Target.ToString();
    }
}
