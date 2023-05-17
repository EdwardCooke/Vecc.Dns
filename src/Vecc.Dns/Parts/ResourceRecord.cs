namespace Vecc.Dns.Parts
{
    public class ResourceRecord : PartBase
    {
        public Name Name { get; set; } = new Name();
        public uint TTL { get; set; } = 0;
        public RData? Data { get; set; }

        public ResourceRecord()
        {
            Name = new Name();
        }

        public override bool Read(MemoryStream stream, ILogger logger)
        {
            if (!Name.Read(stream, logger))
            {
                logger.LogWarning("Unable to read name in {@class}", nameof(ResourceRecord));
                return false;
            }

            if (!TryReadUShort(stream, out var dataType))
            {
                logger.LogWarning("Unable to read data type in {@class}", nameof(ResourceRecord));
                return false;
            }

            var rdataClasses = RData.GetRDataClasses(dataType);
            var dataClass = (ushort)255;
            var readClass = true;

            if (rdataClasses.Length == 1 && rdataClasses[0] == 255)
            {
                logger.LogVerbose("Using any data class for type {@type} in {@class}", dataType, nameof(ResourceRecord));
                Data = RData.GetRData(255, dataType);
            }
            else
            {
                if (!TryReadUShort(stream, out dataClass))
                {
                    logger.LogWarning("Unable to read data class in {@class}", nameof(ResourceRecord));
                    return false;
                }

                logger.LogVerbose("Using data class {@dataclass}", dataClass);
                Data = RData.GetRData(dataClass, dataType);
                if (Data != null && Data.HasClass)
                {
                    Data.Class = (Classes)dataClass;
                }
                readClass = false;
            }

            if (Data == null)
            {
                logger.LogWarning("Unsupported data type and class: {@datatype} {@dataclass}", dataType, dataClass);
                return false;
            }

            if (Data.HasTTL)
            {
                if (!TryReadUInt(stream, out var ttl))
                {
                    return false;
                }

                TTL = ttl;
            }

            if (Data.HasClass && readClass)
            {
                if (!TryReadUShort(stream, out dataClass))
                {
                    logger.LogWarning("Unable to read data class.");
                    return false;
                }

                logger.LogVerbose("Using data class {@dataclass}", dataClass);
                Data.Class = (Classes)dataClass;
            }

            logger.LogVerbose("Using RData type {@type}", Data.GetType().Name.ToString());

            if (!Data.Read(stream, logger))
            {
                logger.LogWarning("Unable to read RData in {@class}", nameof(ResourceRecord));
                return false;
            }

            logger.LogVerbose("{@class} read");
            return true;
        }

        public override bool Write(MemoryStream stream, ILogger logger)
        {
            if (Data == null)
            {
                logger.LogWarning("Unable to write data, it is not set in {@class}", nameof(ResourceRecord));
                return false;
            }

            if (!Name.Write(stream, logger))
            {
                logger.LogWarning("Unable to write name in {@class}", nameof(ResourceRecord));
                return false;
            }

            Write(stream, (ushort)Data.Type);

            if (Data.HasClass)
            {
                logger.LogVerbose("RData has class, writing it");
                Write(stream, (ushort)Data.Class);
            }

            if (Data.HasTTL)
            {
                logger.LogVerbose("RData has TTL, writing it");
                Write(stream, TTL);
            }

            if (!Data.Write(stream, logger))
            {
                logger.LogWarning("Unable to write data in {@class}", nameof(ResourceRecord));
                return false;
            }

            logger.LogVerbose("{@class} written", nameof(ResourceRecord));
            return true;
        }

        public string ToShortString() => $"{Data?.GetType().Name ?? "NULL"} {Data?.ToShortString() ?? "NULL"}";

        public override string ToString() => $"{Data?.GetType().Name ?? "NULL"} {Data}";
    }
}
