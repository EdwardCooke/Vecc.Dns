using System.Buffers.Binary;
using System.Text.RegularExpressions;

namespace Vecc.Dns.Parts
{
    public class Name : PartBase
    {
        private readonly Regex _hostnameRegex = new Regex(@"^([a-zA-Z0-9])([a-zA-Z0-9]|\\|-|\.)*([a-zA-Z0-9])$");
        private string? _value;
        public string? Value
        {
            get => _value;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (!_hostnameRegex.IsMatch(value))
                    {
                        throw new ArgumentException(@"Value must start with [a-z][0-9], contain only letters, digits, escape character \ or periods and end with [a-z][0-9]", nameof(value)) {  Source = value };
                    }
                }
                _value = value;
            }
        }

        public override bool Read(MemoryStream stream, ILogger logger)
        {
            var value = new List<string>();
            var pointer = -1;

            while (stream.Position <= stream.Length)
            {
                var length = stream.ReadByte();
                if (length == -1)
                {
                    logger.LogWarning("Unexpected end of stream while reading {@class}", nameof(Name));
                    return false;
                }

                if (length == 0)
                {
                    //end of the name
                    break;
                }

                //check if it's a pointer, pointers are end of names
                if ((length & 0b11000000) == 0b11000000)
                {
                    var bytes = new byte[2]
                    {
                        (byte)(length & 0b00111111), (byte)stream.ReadByte()
                    };
                    pointer = BinaryPrimitives.ReadUInt16BigEndian(bytes);

                    break;
                }

                var s = string.Empty;
                for (var _ = 0; _ < length; _++)
                {
                    var b = stream.ReadByte();
                    if (b == -1)
                    {
                        //we hit the end of the stream
                        return false;
                    }
                    s += (char)b;
                }
                value.Add(s);
            }

            Value = string.Join('.', value);

            if (pointer > 0)
            {
                var currentPosition = stream.Position;
                if (pointer > currentPosition)
                {
                    //we can't seek to a position farther than we are
                    return false;
                }

                //seek to the pointer, and read the rest of that name, this will recursively go up the chain if there is more pointers.
                stream.Seek(pointer, SeekOrigin.Begin);
                var name = new Name();
                name.Read(stream, logger);

                if (Value == string.Empty)
                {
                    Value = name.Value;
                }
                else
                {
                    Value = $"{Value}.{name.Value}";
                }

                stream.Position = currentPosition;
            }

            logger.LogVerbose("Name read {@value}", Value);
            return true;
        }

        public ushort Length
        {
            get
            {
                if (Value == null)
                {
                    return 0;
                }
                return (ushort)(Value.Length + 1);
            }
        }

        public override bool Write(MemoryStream stream, ILogger logger)
        {
            if (Value == null)
            {
                logger.LogWarning("Value must be set in {@class}", nameof(Name));
                return false;
            }

            // convert escaped periods to something that should never be in the value

            if (Value != string.Empty)
            {
                var toWrite = Value.Replace("\\.", "\\\t");
                var parts = toWrite.Split(".");
                foreach (var part in parts)
                {
                    stream.WriteByte((byte)part.Length);
                    foreach (var c in part)
                    {
                        // if the current char is \t then write out a . and skip this character
                        if (c == '\t')
                        {
                            stream.WriteByte((byte)'.');
                            continue;
                        }

                        stream.WriteByte((byte)c);
                    }
                }
            }

            stream.WriteByte(0);

            logger.LogVerbose("{@class} written", nameof(Name));
            return true;
        }

        public override string ToString() => Value ?? "NULL";

        public static implicit operator Name(string? value) => new Name { Value = value };
        public static implicit operator string?(Name value) => value.Value;
    }
}
