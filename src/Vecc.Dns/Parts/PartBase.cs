using System.Buffers.Binary;

namespace Vecc.Dns.Parts
{
    public abstract class PartBase
    {
        public virtual bool Read(MemoryStream stream, ILogger logger)
        {
            throw new NotImplementedException();
        }

        public virtual bool Write(MemoryStream stream, ILogger logger)
        {
            throw new NotImplementedException();
        }

        public void Write(MemoryStream stream, uint value)
        {
            var buffer = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
            stream.Write(buffer);
        }

        public void Write(MemoryStream stream, ushort value)
        {
            var buffer = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
            stream.Write(buffer);
        }

        public bool TryReadUInt(MemoryStream stream, out uint result)
        {
            var buffer = new byte[4];
            if (stream.Read(buffer) != 4)
            {
                result = default;
                return false;
            }

            result = BinaryPrimitives.ReadUInt32BigEndian(buffer);
            return true;
        }

        public bool TryReadUShort(MemoryStream stream, out ushort result)
        {
            var buffer = new byte[2];
            if (stream.Read(buffer) != 2)
            {
                result = default;
                return false;
            }

            result = BinaryPrimitives.ReadUInt16BigEndian(buffer);
            return true;
        }
    }
}
