namespace Vecc.Dns.Parts
{
    public class Question : PartBase
    {
        public Name Name { get; set; }
        public ResourceRecordTypes QuestionType { get; set; }
        public Classes Class { get; set; }

        public Question()
        {
            Name = new Name();
        }

        public override bool Read(MemoryStream stream, ILogger logger)
        {
            if (!Name.Read(stream, logger))
            {
                logger.LogWarning("Unable to read name in {@class}", nameof(Question));
                return false;
            }

            if (!TryReadUShort(stream, out var value))
            {
                logger.LogWarning("Unable to read question type in {@class}", nameof(Question));
                return false;
            }
            QuestionType = (ResourceRecordTypes)value;

            if (!TryReadUShort(stream, out value))
            {
                logger.LogWarning("Unable to read class in {@class}", nameof(Question));
                return false;
            }
            Class = (Classes)value;

            logger.LogVerbose("{@class} read, Name: {@Name}, QuestionType: {@QuestionType}, Class: {@Class}", nameof(Question), Name, QuestionType, Class);
            return true;
        }

        public override bool Write(MemoryStream stream, ILogger logger)
        {
            if (!Name.Write(stream, logger))
            {
                logger.LogWarning("Unable to write name in {@class}", nameof(Question));
                return false;
            }

            Write(stream, (ushort)QuestionType);
            Write(stream, (ushort)Class);

            return true;
        }

        public override string ToString() => $"Name: {Name} QuestionType: {QuestionType} Class: {Class}";

        public string ToShortString() => $"{Name} {QuestionType} {Class}";
    }
}
