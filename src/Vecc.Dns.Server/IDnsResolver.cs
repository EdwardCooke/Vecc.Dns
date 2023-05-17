using Vecc.Dns.Parts;

namespace Vecc.Dns.Server
{
    public interface IDnsResolver
    {
        Task<Packet?> GetEmptyResponseAsync(Packet packet)
        {
            var responseHeader = new Header
            {
                AnswerCount = 0,
                AdditionalRecordCount = 0,
                Authenticated = false,
                AuthoritativeServer = false,
                Id = packet.Header.Id,
                NameserverCount = 0,
                OpCode = OpCodes.Query,
                PacketType = PacketType.Response,
                QuestionCount = 1,
                ResponseCode = ResponseCodes.NoError,
                RecursionAvailable = false,
                RecursionDesired = packet.Header.RecursionDesired,
                Truncated = false,
            };

            var result = new Packet
            {
                Header = responseHeader,
                Questions = packet.Questions
            };

            return Task.FromResult<Packet?>(result);
        }

        Task<Packet?> ResolveAsync(Packet incoming);
    }
}
