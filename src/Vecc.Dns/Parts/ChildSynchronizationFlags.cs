namespace Vecc.Dns.Parts
{
    public enum ChildSynchronizationFlags
    {
        /// <summary>
        /// Immediately process this CSYNC record.
        /// </summary>
        Immediate = 0,
        /// <summary>
        /// Require a SOA serial number greater than the one specified.
        /// </summary>
        SoaMinimum = 1
    }
}
