namespace Vecc.Dns.Parts.RecordData
{
    public class Ptr : RData
    {
        public override ResourceRecordTypes Type => ResourceRecordTypes.PTR;

        public override Classes Class { get; set; } = Classes.Internet;

        public override bool HasClass => true;

        public override bool HasTTL => true;

        public Name Target { get; set; } = new Name();

        public override bool Read(MemoryStream stream, ILogger logger)
        {
            if (!TryReadUShort(stream, out var length))
            {
                logger.LogWarning("Unable to read length in {@class}", nameof(Ptr));
                return false;
            }

            var currentPosition = stream.Position;
            if (!Target.Read(stream, logger))
            {
                logger.LogWarning("Unable to read target in {@class}", nameof(Ptr));
                return false;
            }


            if (stream.Position != currentPosition + length)
            {
                logger.LogWarning("New position not in the expected spot, expected {@expected}, actual {@} in {@class}", currentPosition + length, stream.Position, nameof(NS));
                return false;
            }

            logger.LogVerbose("{@class} read", nameof(Ptr));
            return true;
        }

        public override bool Write(MemoryStream stream, ILogger logger)
        {
            Write(stream, (ushort)(Target.Length + 1));

            if (!Target.Write(stream, logger))
            {
                logger.LogWarning("Unable to write target in {@class}", nameof(Ptr));
                return false;
            }

            logger.LogVerbose("{@class} written", nameof(Ptr));
            return true;
        }

        public override string ToShortString() => ToString();

        public override string ToString() => Target.ToString();
    }
}
