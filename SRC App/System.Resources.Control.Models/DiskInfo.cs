namespace System.Resources.Control.Models
{
    public class DiskInfo
    {
        public string Model { get; set; }
        public string InterfaceType { get; set; }
        public long Size { get; set; }
        public string MediaType { get; set; }
        public SystemSummaryDiskInfo ToSystemSummaryDiskInfo() => new SystemSummaryDiskInfo
        {
            Model = Model,
            Size = Size
        };
    }
}