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
                Authenticated = false,
                AuthoritativeServer = true,
                Id = incoming.Header.Id,
                NameserverCount = 0,
                OpCode = OpCodes.Query,
                PacketType = PacketType.Response,
                QuestionCount = 1,
                ResponseCode = ResponseCodes.NoError,
                RecursionAvailable = true,
                RecursionDesired = true,
                Truncated = false,
            };

            var answers = new List<ResourceRecord>();
            var question = incoming.Questions.First();
            if (question.QuestionType == ResourceRecordTypes.A)
            {
                answers.Add(new ResourceRecord
                {
                    Name = incoming.Questions.First().Name.Value,
                    TTL = 0,
                    Data = new CName
                    {
                        Class = incoming.Questions.First().Class,
                        Target = "nas.c.lan"
                    }
                });
                answers.Add(new ResourceRecord
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
                });
            };
            if (question.QuestionType == ResourceRecordTypes.NS)
            {
                if (question.Name.Value == "www.google.com")
                {
                    answers.Add(new ResourceRecord
                    {
                        Name = question.Name,
                        TTL = 60,
                        Data = new Soa
                        {
                            Class = Classes.Internet,
                            Expire = 3600000,
                            Refresh = 86400,
                            Retry = 7200,
                            MName = "dns-admin.somethingelse.google.com",
                            Minimum = 172800,
                            RName = "ns1.servers.google.com",
                            Serial = 10
                        }
                    });
                }
                if (question.Name.Value == "google.com")
                {
                    answers.Add(new ResourceRecord
                    {
                        Name = question.Name,
                        TTL = 0,
                        Data = new NS
                        {
                            Class = Classes.Internet,
                            Target = "ns2.google.com"
                        }
                    });
                    answers.Add(new ResourceRecord
                    {
                        Name = "ns2.google.com",
                        TTL = 0,
                        Data = new A
                        {
                            Class = Classes.Internet,
                            IPAddress = IPAddress.Parse("216.239.32.10")
                        }
                    });
                }
            }

            var result = new Packet
            {
                Answers = answers,
                Header = responseHeader,
                Questions = incoming.Questions
            };

            result.Header.AnswerCount = (ushort)answers.Count;
            result.Header.QuestionCount = (ushort)result.Questions.Count;

            return Task.FromResult<Packet?>(result);
        }
    }
}
