namespace Vecc.Dns.Parts
{
    public enum HeaderFlags
    {
        AuthoritativeAnswer = 0b10000,
        TrustedResponse = 0b100000,
        RecursionDenied = 0b1000000,
        RecursionAvailable = 0b10000000,
        Reserved = 0b100000000,
        AuthenticData = 0b1000000000,
        CheckingDisabled = 0b10000000000
    }
}
