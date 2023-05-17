using System.Text;
using Vecc.Dns.Parts;
using Vecc.Dns.Parts.RecordData;

namespace Vecc.Dns
{
    public class Packet
    {
        public Header Header { get; set; } = new Header();
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<ResourceRecord> Answers { get; set; } = new List<ResourceRecord>();
        public ICollection<NS> Nameservers { get; set; } = new List<NS>();
        public ICollection<ResourceRecord> AdditionalRecords { get; set; } = new List<ResourceRecord>();

        public bool Read(MemoryStream stream, ILogger logger)
        {
            Header = new Header();
            if (!Header.Read(stream, logger))
            {
                logger.LogWarning("Unable to read header");
                return false;
            }

            for (var count = 0; count < Header.QuestionCount; count++)
            {
                var question = new Question();

                if (!question.Read(stream, logger))
                {
                    logger.LogWarning("Unable to read question");
                    return false;
                }

                Questions.Add(question);
            }

            for (var count = 0; count < Header.AnswerCount; count++)
            {
                var answer = new ResourceRecord();

                if (!answer.Read(stream, logger))
                {
                    logger.LogWarning("Unable to read answer");
                    return false;
                }

                Answers.Add(answer);
            }

            for (var count = 0; count < Header.NameserverCount; count++)
            {
                var ns = new NS();

                if (!ns.Read(stream, logger))
                {
                    logger.LogWarning("Unable to read nameserver");
                    return false;
                }

                Nameservers.Add(ns);
            }

            for (var count = 0; count < Header.AdditionalRecordCount; count++)
            {
                var record = new ResourceRecord();

                if (!record.Read(stream, logger))
                {
                    logger.LogError("Unable to read additional record");
                    return false;
                }

                AdditionalRecords.Add(record);
            }

            logger.LogVerbose("{@class} read", nameof(Packet));
            return true;
        }

        public bool Write(MemoryStream stream, ILogger logger)
        {
            if (!Header.Write(stream, logger))
            {
                logger.LogWarning("Unable to write header {@header}", Header);
                return false;
            }

            foreach (var question in Questions)
            {
                if (!question.Write(stream, logger))
                {
                    logger.LogWarning("Unable to write question {@question}", question);
                    return false;
                }
            }

            foreach (var answer in Answers)
            {
                if (!answer.Write(stream, logger))
                {
                    logger.LogWarning("Unable to write answer {@answer}", answer);
                    return false;
                }
            }

            foreach (var nameserver in Nameservers)
            {
                if (!nameserver.Write(stream, logger))
                {
                    logger.LogWarning("Unable to write nameserver {@nameserver}", nameserver);
                    return false;
                }
            }

            foreach (var record in AdditionalRecords)
            {
                if (!record.Write(stream, logger))
                {
                    logger.LogWarning("Unable to write additional record {@additionalrecord}", record);
                    return false;
                }
            }

            logger.LogVerbose("{@class} written", nameof(Packet));
            return true;
        }

        public string ToShortString()
        {
            var builder = new StringBuilder();

            builder.AppendFormat("Id: {0}", Header.Id);

            foreach (var question in Questions)
            {
                builder.AppendFormat(" Q:{{{0}}}", question.ToShortString());
            }

            foreach (var answer in Answers)
            {
                builder.AppendFormat(" A:{{{0}}}", answer.ToShortString());
            }

            foreach (var nameserver in Nameservers)
            {
                builder.AppendFormat(" N:{{{0}}}", nameserver.ToShortString());
            }

            foreach (var additional in AdditionalRecords)
            {
                builder.AppendFormat(" AR:{{{0}}}", additional.ToShortString());
            }

            return builder.ToString();
        }
    }
}
