using System.Collections.Generic;

namespace System.Resources.Control.Models
{
    public class RamInfo
    {
        public long TotalCapacity { get; set; }
        public List<RamModuleInfo> Modules { get; set; }

        public SystemSummaryRamInfo ToSystemSummaryRamInfo() => new SystemSummaryRamInfo
        {
            TotalRam = TotalCapacity,
        };
    }
}