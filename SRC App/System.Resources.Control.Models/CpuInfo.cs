namespace System.Resources.Control.Models
{
    public class CpuInfo
    {
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public int NumberOfCores { get; set; }
        public int NumberOfLogicalProcessors { get; set; }
        public int MaxClockSpeed { get; set; }
        public int CurrentClockSpeed { get; set; }
        public string ProcessorId { get; set; }
        public int L2CacheSize { get; set; }
        public int L3CacheSize { get; set; }
        public string Architecture { get; set; }
        public string ProcessorType { get; set; }
        public string Status { get; set; }

        public SystemSummaryCpuInfo ToSystemSummaryCpuInfo() => new SystemSummaryCpuInfo
        {
            Name = Name,
            NumberOfCores = NumberOfCores,
            NumberOfLogicalProcessors = NumberOfLogicalProcessors,
        };
    }
}