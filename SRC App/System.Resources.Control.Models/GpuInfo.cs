namespace System.Resources.Control.Models
{
    public class GpuInfo
    {
        public string Name { get; set; }
        public string AdapterRAM { get; set; }
        public string DriverVersion { get; set; }
        public string VideoProcessor { get; set; }

        public SystemSummaryGpuInfo ToSystemSummaryGpuInfo() => new SystemSummaryGpuInfo
        {
            Name = Name,
            AdapterRAM = AdapterRAM,
        };
    }
}