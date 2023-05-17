namespace Vecc.Dns.Parts.RecordData
{
    public class Soa : RData
    {
        public override ResourceRecordTypes Type => ResourceRecordTypes.SOA;

        public override Classes Class { get; set; } = Classes.Internet;

        public override bool HasClass => true;

        public override bool HasTTL => true;

        public Name MName { get; set; } = new Name();
        public Name RName { get; set; } = new Name();
        public uint Serial { get; set; }
        public uint Refresh { get; set; }
        public uint Retry { get; set; }
        public uint Expire { get; set; }
        public uint Minimum { get; set; }

        public override bool Read(MemoryStream stream, ILogger logger)
        {
            if (!MName.Read(stream, logger))
            {
                logger.LogWarning("Unable to read MName in {@class}", nameof(Soa));
                return false;
            }

            if (!RName.Read(stream, logger))
            {
                logger.LogWarning("Unable to read RName in {@class}", nameof(Soa));
                return false;
            }

            if (!TryReadUInt(stream, out var value))
            {
                logger.LogWarning("Unable to read Serial in {@class}", nameof(Soa));
                return false;
            }
            Serial = value;

            if (!TryReadUInt(stream, out value))
            {
                logger.LogWarning("Unable to read Refresh in {@class}", nameof(Soa));
                return false;
            }
            Refresh = value;

            if (!TryReadUInt(stream, out value))
            {
                logger.LogWarning("Unable to read Retry in {@class}", nameof(Soa));
                return false;
            }
            Retry = value;

            if (!TryReadUInt(stream, out value))
            {
                logger.LogWarning("Unable to read Expire in {@class}", nameof(Soa));
                return false;
            }
            Expire = value;

            if (!TryReadUInt(stream, out value))
            {
                logger.LogWarning("Unable to read Minimum in {@class}", nameof(Soa));
                return false;
            }
            Minimum = value;

            logger.LogVerbose("{@class} read. Serial {@serial}, Refresh {@refresh}, Retry {@retry}, Expire {@expire}, Minimum {@minimum}", nameof(Soa), Serial, Refresh, Retry, Expire, Minimum);
            return true;
        }

        public override string ToShortString() => ToString();

        public override string ToString() => $"MName {MName} RName {RName} Serial {Serial}, Refresh {Refresh}, Retry {Retry}, Expire {Expire}, Minimum {Minimum}";
    }
}
