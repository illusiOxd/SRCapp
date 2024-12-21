using System;
using System.Collections.Generic;
using System.Linq;
using System.Management; // Added this using directive
using System.Resources.Control.Models;

namespace System.Resources.Control.Services
{
    public class SystemInfoService : ISystemInfoService
    {
        public CpuInfo GetDetailedCpuInfo() => GetManagementObject("Win32_Processor", obj => new CpuInfo
        {
            Name = obj["Name"]?.ToString(),
            Manufacturer = obj["Manufacturer"]?.ToString(),
            NumberOfCores = Convert.ToInt32(obj["NumberOfCores"]),
            NumberOfLogicalProcessors = Convert.ToInt32(obj["NumberOfLogicalProcessors"]),
            MaxClockSpeed = Convert.ToInt32(obj["MaxClockSpeed"]),
            CurrentClockSpeed = Convert.ToInt32(obj["CurrentClockSpeed"]),
            ProcessorId = obj["ProcessorId"]?.ToString(),
            L2CacheSize = Convert.ToInt32(obj["L2CacheSize"]),
            L3CacheSize = Convert.ToInt32(obj["L3CacheSize"]),
            Architecture = obj["Architecture"]?.ToString(),
            ProcessorType = obj["ProcessorType"]?.ToString(),
            Status = obj["Status"]?.ToString()
        });
        public List<GpuInfo> GetGpuInfo() => GetManagementObjects("Win32_VideoController", obj =>
        {
            long.TryParse(obj["AdapterRAM"]?.ToString(), out long adapterRam);
            return new GpuInfo
            {
                Name = obj["Name"]?.ToString(),
                AdapterRAM = adapterRam == 0 ? "Not Available" : $"{adapterRam / (1024 * 1024)} MB",
                DriverVersion = obj["DriverVersion"]?.ToString(),
                VideoProcessor = obj["VideoProcessor"]?.ToString()
            };
        });

        public RamInfo GetRamInfo()
        {
            return GetManagementObject("Win32_PhysicalMemory", objCollection =>
            {
                long totalCapacity = 0;
                var modules = new List<RamModuleInfo>();
                foreach (var obj in objCollection)
                {
                    totalCapacity += Convert.ToInt64(obj["Capacity"]);
                    modules.Add(new RamModuleInfo
                    {
                        Capacity = Convert.ToInt64(obj["Capacity"]) / (1024 * 1024 * 1024),
                        Manufacturer = obj["Manufacturer"]?.ToString(),
                        Speed = Convert.ToInt32(obj["Speed"]),
                        PartNumber = obj["PartNumber"]?.ToString()
                    });
                }
                return new RamInfo
                {
                    TotalCapacity = totalCapacity / (1024 * 1024 * 1024),
                    Modules = modules
                };
            });
        }

        public List<DiskInfo> GetDiskInfo() => GetManagementObjects("Win32_DiskDrive", obj => new DiskInfo
        {
            Model = obj["Model"]?.ToString(),
            InterfaceType = obj["InterfaceType"]?.ToString(),
            Size = Convert.ToInt64(obj["Size"]) / (1024 * 1024 * 1024),
            MediaType = obj["MediaType"]?.ToString()
        });
        public List<NetworkInfo> GetNetworkInfo() => GetManagementObjects("Win32_NetworkAdapter where NetEnabled = true", obj => new NetworkInfo
        {
            Name = obj["Name"]?.ToString(),
            MACAddress = obj["MACAddress"]?.ToString(),
            Speed = Convert.ToInt64(obj["Speed"])
        });
        public SystemSummaryCpuInfo GetSystemSummaryCpuInfo() => GetDetailedCpuInfo().ToSystemSummaryCpuInfo();
        public SystemSummaryRamInfo GetSystemSummaryRamInfo() => GetRamInfo().ToSystemSummaryRamInfo();
        public SystemSummaryGpuInfo GetSystemSummaryGpuInfo() => GetGpuInfo().FirstOrDefault()?.ToSystemSummaryGpuInfo() ?? new SystemSummaryGpuInfo();
        public SystemSummaryDiskInfo GetSystemSummaryDiskInfo() => GetDiskInfo().FirstOrDefault()?.ToSystemSummaryDiskInfo() ?? new SystemSummaryDiskInfo();
        public TemperatureInfo GetTemperatureInfo() => GetManagementObject("MSAcpi_ThermalZoneTemperature", obj =>
        {
            if (obj != null && double.TryParse(obj["CurrentTemperature"]?.ToString(), out double tempKelvin))
            {
                return new TemperatureInfo
                {
                    HasTemperatureData = true,
                    TemperatureCelsius = (tempKelvin - 2732) / 10.0
                };
            }
            return new TemperatureInfo { HasTemperatureData = false };
        }, "root\\WMI");

        private T GetManagementObject<T>(string query, Func<ManagementObject, T> mapper, string scope = null)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(scope ?? string.Empty, $"SELECT * FROM {query}");
                return searcher.Get().Cast<ManagementObject>().FirstOrDefault() is { } obj ? mapper(obj) : default;

            }
            catch (Exception)
            {
                return default;
            }
        }

        private T GetManagementObject<T>(string query, Func<ManagementObjectCollection, T> mapper, string scope = null)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(scope ?? string.Empty, $"SELECT * FROM {query}");
                return mapper(searcher.Get());

            }
            catch (Exception)
            {
                return default;
            }
        }
        private List<T> GetManagementObjects<T>(string query, Func<ManagementObject, T> mapper, string scope = null)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(scope ?? string.Empty, $"SELECT * FROM {query}");
                return searcher.Get().Cast<ManagementObject>().Select(mapper).ToList();
            }
            catch (Exception)
            {
                return new List<T>();
            }
        }
    }
}