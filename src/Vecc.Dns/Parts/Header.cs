using System.Buffers.Binary;

namespace Vecc.Dns.Parts
{
    public class Header : PartBase
    {
        public ushort Id { get; set; }
        public PacketType PacketType { get; set; }
        public OpCodes OpCode { get; set; }
        public bool Authenticated { get; set; }
        public bool AuthoritativeServer { get; set; }
        public bool CheckingDisabled { get; set; }
        public bool Truncated { get; set; }
        public bool RecursionDesired { get; set; }
        public bool RecursionAvailable { get; set; }
        public byte Reserved { get; set; }
        public ResponseCodes ResponseCode { get; set; }
        public ushort QuestionCount { get; set; }
        public ushort AnswerCount { get; set; }
        public ushort NameserverCount { get; set; }
        public ushort AdditionalRecordCount { get; set; }

        public override bool Read(MemoryStream stream, ILogger logger)
        {
            var buffer = new byte[4];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead != buffer.Length)
            {
                logger.LogWarning("Unable to read Id, invalid length in {@class}, expected {@expected}, actual {@actual}", nameof(Header), 4, bytesRead);
                return false;
            }

            Id = BinaryPrimitives.ReadUInt16BigEndian(buffer.Take(2).ToArray());// BitConverter.ToUInt16(buffer, 0);
            var packed = buffer[2];

            PacketType = (PacketType)(packed & 0b10000000);
            OpCode = (OpCodes)((packed & 0b01111000) >> 3);
            AuthoritativeServer = (packed & 0b00001000) == 0b00001000;
            Truncated = (packed & 0b00000100) == 0b00000100;
            RecursionDesired = (packed & 0b00000010) == 0b00000010;

            packed = buffer[3];
            RecursionAvailable = (packed & 0b00000001) == 0b00000001;

            if ((packed & (1 << 6)) != 0)
            {
                logger.LogWarning("Reserved bit is not false in {@class}", nameof(Header));
                return false;
            }

            ResponseCode = (ResponseCodes)(packed & 0b1111);

            if (!TryReadUShort(stream, out var value))
            {
                logger.LogWarning("Unable to read questioncount in {@class}", nameof(Header));
                return false;
            }
            QuestionCount = value;

            if (!TryReadUShort(stream, out value))
            {
                logger.LogWarning("Unable to read answercount in {@class}", nameof(Header));
                return false;
            }
            AnswerCount = value;

            if (!TryReadUShort(stream, out value))
            {
                logger.LogWarning("Unable to read nameservercount in {@class}", nameof(Header));
                return false;
            }
            NameserverCount = value;

            if (!TryReadUShort(stream, out value))
            {
                logger.LogWarning("Unable to read additionalrecordcount in {@class}", nameof(Header));
                return false;
            }
            AdditionalRecordCount = value;

            logger.LogVerbose("{@class} read, Id {@id}, " +
                "PacketType {@packettype}, " +
                "OpCode {@opcode}, " +
                "AuthoritativeServer {@authoritative}, " +
                "Truncated {@truncated}, " +
                "RecursionDesired: {@recursiondesired}, " +
                "Packed: {@packed}, " +
                "RecursionAvailable: {@recursionavailable}, " +
                "ResponseCode: {@responseCode}, " +
                "QuestionCount: {@questioncount}, " +
                "AnswerCount: {@answerCount}, " +
                "NameserverCount: {@nameserverCount}, " +
                "AdditionalRecordCount: {@additionalrecordcount}",
                Id,
                PacketType,
                OpCode,
                AuthoritativeServer,
                Truncated,
                RecursionDesired,
                packed,
                RecursionAvailable,
                ResponseCode,
                QuestionCount,
                AnswerCount,
                NameserverCount,
                AdditionalRecordCount);
            return true;
        }

        public override bool Write(MemoryStream stream, ILogger logger)
        {
            Write(stream, Id);

            var b = (byte)0;
            if (PacketType == PacketType.Response)
            {
                b = 0b10000000;
            }
            //var b = (byte)PacketType;
            b = (byte)(b | ((int)OpCode << 3));
            if (AuthoritativeServer)
            {
                b = (byte)(b | 0b00001000);
            }
            if (Truncated)
            {
                b = (byte)(b | 0b00000100);
            }
            if (RecursionDesired)
            {
                b = (byte)(b | 0b00000010);
            }
            stream.WriteByte(b);

            b = 0;
            if (RecursionAvailable)
            {
                b = (byte)(b | 0b00000001);
            }
            if (Authenticated)
            {
                b = (byte)(b | 0b00100000);
            }
            if (CheckingDisabled)
            {
                b = (byte)(b | 0b00010000);
            }
            stream.WriteByte(b);

            Write(stream, QuestionCount);
            Write(stream, AnswerCount);
            Write(stream, NameserverCount);
            Write(stream, AdditionalRecordCount);

            logger.LogVerbose("{@class} written", nameof(Header));
            return true;
        }

        public override string ToString()
        {
            return $"Id: {Id}, " +
                $"PacketType: {PacketType}, " +
                $"OpCode: {OpCode}, " +
                $"AuthoritativeServer: {AuthoritativeServer}, " +
                $"Truncated: {Truncated}, " +
                $"RecursionDesired: {RecursionDesired}, " +
                $"RecursionAvailable: {RecursionAvailable}, " +
                $"Reserved: {Reserved}, " +
                $"ResponseCode: {ResponseCode}, " +
                $"QuestionCount: {QuestionCount}, " +
                $"AnswerCount: {AnswerCount}, " +
                $"NameserverCount: {NameserverCount}, " +
                $"AdditionalRecordCount: {AdditionalRecordCount}";
        }
    }
}
