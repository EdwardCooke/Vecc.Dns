using Vecc.Dns.Parts.RecordData;

namespace Vecc.Dns.Parts
{
    public abstract class RData : PartBase
    {
        public abstract ResourceRecordTypes Type { get; }
        public virtual Classes Class { get; set; }
        public abstract bool HasTTL { get; }
        public abstract bool HasClass { get; }
        public abstract string ToShortString();

        private static readonly Dictionary<ushort, Dictionary<ushort, Func<RData>>> _rdataTypes;

        static RData()
        {
            _rdataTypes = new Dictionary<ushort, Dictionary<ushort, Func<RData>>>();

            AddRDataType(() => new A());
            AddRDataType(() => new CName());
            AddRDataType(() => new NS());
            AddRDataType(() => new Ptr());
            AddRDataType(() => new Soa());
            AddRDataType(() => new Opt());
        }

        private static void AddRDataType(Func<RData> constructor)
        {
            var rdata = constructor();
            var types = _rdataTypes.GetValueOrDefault((ushort)rdata.Type, new Dictionary<ushort, Func<RData>>());
            _rdataTypes[(ushort)rdata.Type] = types;
            types[(ushort)rdata.Class] = constructor;
        }

        public static RData? GetRData(ushort dataClass, ushort dataType)
        {
            if (_rdataTypes.TryGetValue(dataType, out var classes))
            {
                if (classes.TryGetValue(dataClass, out var constructor))
                {
                    return constructor();
                }
                else if (classes.TryGetValue(255, out constructor))
                {
                    return constructor();
                }

                // data class not found
                return null;
            }

            // data type not found
            return null;
        }

        public static ushort[] GetRDataClasses(ushort dataType)
        {
            if (_rdataTypes.TryGetValue(dataType, out var classes))
            {
                return classes.Keys.ToArray();
            }

            return new ushort[0];
        }
    }
}
