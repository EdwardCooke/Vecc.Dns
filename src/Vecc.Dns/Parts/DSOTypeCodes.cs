namespace Vecc.Dns.Parts
{
    public enum DSOTypeCodes
    {
        KeepAlive = 1,
        RetryDelay = 2,
        EncryptionPadding = 3,
        Subscribe = 0x40,
        Push = 0x41,
        Unsubscribe = 0x42,
        Reconfirm = 0x43
    }
}
