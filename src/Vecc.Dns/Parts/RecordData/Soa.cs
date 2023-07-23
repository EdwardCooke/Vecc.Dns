namespace Vecc.Dns.Parts.RecordData
{
    public class Soa : RData
    {
        public override ResourceRecordTypes Type => ResourceRecordTypes.SOA;

        public override Classes Class { get; set; } = Classes.Internet;

        public override bool HasClass => true;

        public override bool HasTTL => true;

        /// <summary>
        /// Responsible mail box name
        /// </summary>
        public Name MName { get; set; } = new Name();

        /// <summary>
        /// Primary responsible nameserver
        /// </summary>
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

        public override bool Write(MemoryStream stream, ILogger logger)
        {
            var length = 2 + //data length
                        MName.Length +
                        RName.Length +
                        4 + //serial
                        4 + //refresh
                        4 + //retry
                        4 + //expire
                        4; //minimum

            Write(stream, (ushort)length);

            if (!RName.Write(stream, logger))
            {
                logger.LogWarning("Unable to write RName in {@class}", nameof(Soa));
                return false;
            }

            if (!MName.Write(stream, logger))
            {
                logger.LogWarning("Unable to write MName in {@class}", nameof(Soa));
                return false;
            }

            Write(stream, Serial);
            Write(stream, Refresh);
            Write(stream, Retry);
            Write(stream, Expire);
            Write(stream, Minimum);

            logger.LogVerbose("{@class} written", nameof(Soa));
            return true;
        }

        public override string ToShortString() => ToString();

        public override string ToString() => $"MName {MName} RName {RName} Serial {Serial}, Refresh {Refresh}, Retry {Retry}, Expire {Expire}, Minimum {Minimum}";
    }
}
