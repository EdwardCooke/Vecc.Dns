using System.Net;
using Vecc.Dns.Parts;
using Vecc.Dns.Parts.RecordData;
using Vecc.Dns.Server;

namespace Vecc.Dns.Example
{
    public class TestDnsResolver : IDnsResolver
    {
        public Task<Packet?> ResolveAsync(Packet incoming)
        {
            var responseHeader = new Header
            {
                AnswerCount = 2,
                AdditionalRecordCount = 0,
                Authenticated = false,
                AuthoritativeServer = false,
                Id = incoming.Header.Id,
                NameserverCount = 0,
                OpCode = OpCodes.Query,
                PacketType = PacketType.Response,
                QuestionCount = 1,
                ResponseCode = ResponseCodes.NoError,
                RecursionAvailable = false,
                RecursionDesired = incoming.Header.RecursionDesired,
                Truncated = false,
            };

            var answers = new List<ResourceRecord>
            {
                new ResourceRecord
                {
                    Name = new Name { Value = incoming.Questions.First().Name.Value },
                    TTL = 0,
                    Data = new CName
                    {
                        Class = incoming.Questions.First().Class,
                        Target = new Name { Value = "nas.c.lan" }
                    }
                },
                new ResourceRecord
                {
                    Name = new Name
                    {
                        Value = "nas.c.lan"
                    },
                    TTL = 0,
                    Data = new A
                    {
                        Class = incoming.Questions.First().Class,
                        IPAddress = IPAddress.Parse("192.168.0.189")
                    }
                }
            };

            var result = new Packet
            {
                Answers = answers,
                Header = responseHeader,
                Questions = incoming.Questions
            };

            return Task.FromResult<Packet?>(result);
        }
    }
}
