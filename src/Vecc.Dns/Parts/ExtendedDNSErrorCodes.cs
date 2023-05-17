namespace Vecc.Dns.Parts
{
    public enum ExtendedDNSErrorCodes
    {
        Other = 0,
        UnsupportedDNSKeyAlgorithm = 1,
        UnsupportedDSDigestType = 2,
        StaleAnswer = 3,
        ForgedAnswer = 4,
        DNSSecIndeterminate = 5,
        DNSSECBogus = 6,
        SignatureExpired = 7,
        SignatureNotYetValid = 8,
        DNSKEYMissing = 9,
        RRSIGsMissing = 10,
        NoZoneKeyBitSet = 11,
        NSECMissing = 12,
        CachedError = 13,
        NotReady = 14,
        Blocked = 15,
        Censored = 16,
        Filtered = 17,
        Prohibited = 18,
        StaleNXDomainAnswer = 19,
        NotAuthoritative = 20,
        NotSupported = 21,
        NoReachableAuthority = 22,
        NetworkError = 23,
        InvalidData = 24,
        SignatureExpiredBeforeValid = 25,
        TooEarly = 26,
        UnsupportedNSEC3IterationsValue = 27,
        UnableToConformToPolicy = 28
    }
}
