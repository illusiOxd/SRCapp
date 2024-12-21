using System.Collections.Generic;
using System.Resources.Control.Models;

namespace System.Resources.Control.Services
{
    public interface ISystemInfoService
    {
        CpuInfo GetDetailedCpuInfo();
        List<GpuInfo> GetGpuInfo();
        RamInfo GetRamInfo();
        List<DiskInfo> GetDiskInfo();
        List<NetworkInfo> GetNetworkInfo();
        SystemSummaryCpuInfo GetSystemSummaryCpuInfo();
        SystemSummaryRamInfo GetSystemSummaryRamInfo();
        SystemSummaryGpuInfo GetSystemSummaryGpuInfo();
        SystemSummaryDiskInfo GetSystemSummaryDiskInfo();
        TemperatureInfo GetTemperatureInfo();
    }
}