using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Resources.Control.Models;
using System.Resources.Control.Services;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Security.Permissions;
using OpenHardwareMonitor.Hardware;
using System.Management;
using System.Windows.Shapes;
using System.Threading;
using System.Net.NetworkInformation;
using System.Diagnostics;


namespace SystemResourcesControlWpf
{
    // PerformanceMonitorWindow.xaml.cs
    public partial class PerformanceMonitorWindow : Window, INotifyPropertyChanged
    {
        private double _cpuUsage;
        private double _ramUsage;
        private double _diskUsage;
        private double _networkUsage;

        public double CpuUsage
        {
            get => _cpuUsage;
            set
            {
                _cpuUsage = value;
                OnPropertyChanged(nameof(CpuUsage));
            }
        }

        public double RamUsage
        {
            get => _ramUsage;
            set
            {
                _ramUsage = value;
                OnPropertyChanged(nameof(RamUsage));
            }
        }

        public double DiskUsage
        {
            get => _diskUsage;
            set
            {
                _diskUsage = value;
                OnPropertyChanged(nameof(DiskUsage));
            }
        }

        public double NetworkUsage
        {
            get => _networkUsage;
            set
            {
                _networkUsage = value;
                OnPropertyChanged(nameof(NetworkUsage));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PerformanceMonitorWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly ISystemInfoService _systemInfoService;
        private bool isStopped;
        private readonly Model _model;
        private System.Threading.Timer _timer;
        private double _canvasWidth;
        private PerformanceMonitorWindow _performanceMonitorWindow;

        public double CurrentCpuUsageValue { get; private set; }
        public double CurrentRamUsageValue { get; private set; }
        public double CurrentDiskUsageValue { get; private set; }
        public double CurrentNetworkUsageValue { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _systemInfoService = new SystemInfoService();
            _model = new Model(_systemInfoService);

            InitializeNetworkCounters();
            StartMonitoring();
            Closed += MainWindow_Closed;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _canvasWidth = CpuUsageCanvas.ActualWidth;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _timer?.Dispose();
            _timer = null;
        }

        private void StartMonitoring()
        {
            _timer = new System.Threading.Timer(UpdateAllResources, null, TimeSpan.Zero, TimeSpan.FromSeconds(0.5));
        }

        private void UpdateAllResources(object state)
        {
            if (isStopped) return;

            double cpuUsage = GetCurrentCpuUsage();
            double ramUsage = GetCurrentRamUsage();
            double diskUsage = GetCurrentDiskUsage();
            double networkUsage = GetCurrentNetworkUsage();

            CurrentCpuUsageValue = cpuUsage;
            CurrentRamUsageValue = ramUsage;
            CurrentDiskUsageValue = diskUsage;
            CurrentNetworkUsageValue = networkUsage;
            
            Dispatcher.Invoke((Action)(() =>
            {
                CpuUsageTextBlock.Text = $"CPU Usage: {cpuUsage:F2}%";
                UpdateCpuUsageChart(cpuUsage);

                if (_performanceMonitorWindow != null && _performanceMonitorWindow.IsVisible)
                {
                    _performanceMonitorWindow.CpuUsage = CurrentCpuUsageValue;
                    _performanceMonitorWindow.RamUsage = CurrentRamUsageValue;
                    _performanceMonitorWindow.DiskUsage = CurrentDiskUsageValue;
                    _performanceMonitorWindow.NetworkUsage = CurrentNetworkUsageValue;
                }
            }));
        }

        private void UpdateCpuUsageChart(double cpuUsage)
        {
            var points = CpuUsagePolyline.Points;
            points.Add(new Point(points.Count, CpuUsageCanvas.ActualHeight - (cpuUsage / 100.0 * CpuUsageCanvas.ActualHeight)));

            if (points.Count > _canvasWidth)
            {
                points.RemoveAt(0);
                for (int i = 0; i < points.Count; i++)
                {
                    points[i] = new Point(points[i].X - 1, points[i].Y);
                }
            }
        }

        private double GetCurrentCpuUsage()
        {
            double totalCpu = 0;
            foreach (var counter in _model.ThreadCounters)
            {
                totalCpu += counter.NextValue();
            }
            return _model.ThreadCounters.Count > 0 ? totalCpu / _model.ThreadCounters.Count : 0;
        }

        private double GetCurrentRamUsage()
        {
            var ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            return ramCounter.NextValue();
        }

        private double GetCurrentDiskUsage()
        {
            try
            {
                var diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
                diskCounter.NextValue();
                Thread.Sleep(100); 
                var diskUsage = diskCounter.NextValue();
                diskCounter.Dispose();
                return diskUsage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting disk usage: {ex.Message}");
                return 0;
            }
        }

        private double GetCurrentNetworkUsage()
        {
            if (_networkInterfaces == null || _networkInterfaces.Length == 0)
            {
                return 0;
            }

            long totalBytesSent = 0;
            long totalBytesReceived = 0;

            for (int i = 0; i < _networkInterfaces.Length; i++)
            {
                if (_networkInterfaces[i].OperationalStatus == OperationalStatus.Up &&
                    _networkInterfaces[i].Supports(NetworkInterfaceComponent.IPv4))
                {
                    IPv4InterfaceStatistics stats = _networkInterfaces[i].GetIPv4Statistics();

                    long currentBytesSent = stats.BytesSent;
                    long currentBytesReceived = stats.BytesReceived;

                    long bytesSentDelta = currentBytesSent - _lastBytesSent[i];
                    long bytesReceivedDelta = currentBytesReceived - _lastBytesReceived[i];

                    totalBytesSent += bytesSentDelta;
                    totalBytesReceived += bytesReceivedDelta;

                    // Обновляем последние значения
                    _lastBytesSent[i] = currentBytesSent;
                    _lastBytesReceived[i] = currentBytesReceived;
                }
            }
            
            long result = (totalBytesSent + totalBytesReceived) * 8;
            result = result / 1000000;
            return result;
        }

        private bool IsRunAsAdmin()
        {
            try
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(id);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking admin rights: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void AddOutput(string text, StackPanel panel = null)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 2, 0, 2),
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 14
            };
            if (panel == null)
                OutputPanel.Children.Add(textBlock);
            else
                panel.Children.Add(textBlock);
        }

        private void AddOutputSeparator(StackPanel panel = null)
        {
            var separator = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 0, 1), Margin = new Thickness(0, 5, 0, 5) };
            if (panel == null)
                OutputPanel.Children.Add(separator);
            else
                panel.Children.Add(separator);
        }

        private void AddCard(string header, Action<StackPanel> content)
        {
            GroupBox card = new GroupBox { Header = header, Margin = new Thickness(0, 5, 0, 5) };
            StackPanel panel = new StackPanel();
            card.Content = panel;
            content(panel);
            OutputPanel.Children.Add(card);
        }

        private void ShowPerformanceMonitor_Click(object sender, RoutedEventArgs e)
        {
            if (_performanceMonitorWindow == null || !_performanceMonitorWindow.IsVisible)
            {
                _performanceMonitorWindow = new PerformanceMonitorWindow();
                _performanceMonitorWindow.CpuUsage = CurrentCpuUsageValue;
                _performanceMonitorWindow.RamUsage = CurrentRamUsageValue;
                _performanceMonitorWindow.DiskUsage = CurrentDiskUsageValue;
                _performanceMonitorWindow.NetworkUsage = CurrentNetworkUsageValue;
                _performanceMonitorWindow.Show();
            }
            else
            {
                _performanceMonitorWindow.Focus();
            }
        }

        private void ShowCpuInfo_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();
            AddCard("CPU Information", (panel) => { DisplayDetailedCpuInfo(panel); });
        }

        private void DisplayDetailedCpuInfo(StackPanel panel)
        {
            try
            {
                var cpuInfo = _systemInfoService.GetDetailedCpuInfo();
                AddOutput("Processor Name: " + cpuInfo.Name, panel);
                AddOutput("Manufacturer: " + cpuInfo.Manufacturer, panel);
                AddOutput("Number of Cores: " + cpuInfo.NumberOfCores, panel);
                AddOutput("Number of Logical Processors: " + cpuInfo.NumberOfLogicalProcessors, panel);
                AddOutput("Max Clock Speed: " + cpuInfo.MaxClockSpeed + " MHz", panel);
                AddOutput("Current Clock Speed: " + cpuInfo.CurrentClockSpeed + " MHz", panel);
                AddOutput("Processor ID: " + cpuInfo.ProcessorId, panel);
                AddOutput("L2 Cache Size: " + cpuInfo.L2CacheSize + " KB", panel);
                AddOutput("L3 Cache Size: " + cpuInfo.L3CacheSize + " KB", panel);
                AddOutput("Architecture: " + cpuInfo.Architecture, panel);
                AddOutput("Processor Type: " + cpuInfo.ProcessorType, panel);
                AddOutput("Status: " + cpuInfo.Status, panel);
                AddOutputSeparator(panel);
            }
            catch (Exception ex)
            {
                AddOutput("Error fetching CPU info: " + ex.Message, panel);
            }
        }

        private void ShowGpuInfo_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();
            AddCard("GPU Information", (panel) =>
            {
                try
                {
                    foreach (var gpuInfo in _systemInfoService.GetGpuInfo())
                    {
                        AddOutput("GPU Name: " + gpuInfo.Name, panel);
                        AddOutput("Adapter RAM: " + gpuInfo.AdapterRAM, panel);
                        AddOutput("Driver Version: " + gpuInfo.DriverVersion, panel);
                        AddOutput("Video Processor: " + gpuInfo.VideoProcessor, panel);
                        AddOutputSeparator(panel);
                    }
                }
                catch (Exception ex)
                {
                    AddOutput("Error fetching GPU info: " + ex.Message, panel);
                }
            });
        }

        private void ShowRamInfo_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();
            AddCard("RAM Information", (panel) =>
            {
                try
                {
                    var ramInfos = _systemInfoService.GetRamInfo();
                    AddOutput("Total Installed RAM: " + ramInfos.TotalCapacity + " GB\n", panel);

                    int moduleNumber = 1;
                    foreach (var ramInfo in ramInfos.Modules)
                    {
                        AddOutput("Module " + moduleNumber++ + ":", panel);
                        AddOutput("  Capacity: " + ramInfo.Capacity + " GB", panel);
                        AddOutput("  Manufacturer: " + ramInfo.Manufacturer, panel);
                        AddOutput("  Speed: " + ramInfo.Speed + " MHz", panel);
                        AddOutput("  Part Number: " + ramInfo.PartNumber + "\n", panel);
                        AddOutputSeparator(panel);
                    }
                }
                catch (Exception ex)
                {
                    AddOutput("Error fetching RAM info: " + ex.Message, panel);
                }
            });
        }

        private void ShowDiskInfo_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();
            AddCard("Disk Information", (panel) =>
            {
                try
                {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        AddOutput("Model: " + queryObj["Model"], panel);
                        AddOutput("Interface Type: " + queryObj["InterfaceType"], panel);
                        ulong sizeBytes = (ulong)queryObj["Size"];
                        AddOutput("Size: " + Math.Round(sizeBytes / (1024.0 * 1024.0 * 1024.0), 2) + " GB", panel);
                        AddOutput("Manufacturer: " + queryObj["Manufacturer"], panel);
                        AddOutput("MediaType: " + queryObj["MediaType"], panel); // Может быть null или не всегда точным
                        AddOutput("Serial Number: " + queryObj["SerialNumber"], panel);

                        string deviceId = queryObj["DeviceID"].ToString().Replace("\\\\.\\", "");
                        ManagementObjectSearcher partitionSearcher = new ManagementObjectSearcher(
                            $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{queryObj["DeviceID"]}'}} WHERE AssocClass = Win32_DiskDriveToDiskPartition");

                        foreach (ManagementObject partitionObj in partitionSearcher.Get())
                        {
                            AddOutput("  Partition:", panel);
                            AddOutput("    Name: " + partitionObj["Name"], panel);
                            ulong partitionSizeBytes = (ulong)partitionObj["Size"];
                            AddOutput("    Size: " + Math.Round(partitionSizeBytes / (1024.0 * 1024.0 * 1024.0), 2) + " GB", panel);
                            AddOutput("    Type: " + partitionObj["Type"], panel);

                            ManagementObjectSearcher volumeSearcher = new ManagementObjectSearcher(
                                $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partitionObj["DeviceID"]}'}} WHERE AssocClass = Win32_LogicalDiskToPartition");

                            foreach (ManagementObject volumeObj in volumeSearcher.Get())
                            {
                                AddOutput("    Volume:", panel);
                                AddOutput("      Drive Letter: " + volumeObj["DriveLetter"], panel);
                                AddOutput("      Volume Name: " + volumeObj["VolumeName"], panel);
                                string fileSystem = volumeObj["FileSystem"]?.ToString();
                                AddOutput("      File System: " + fileSystem, panel);
                                ulong volumeSizeBytes = (ulong)volumeObj["Size"];
                                AddOutput("      Size: " + Math.Round(volumeSizeBytes / (1024.0 * 1024.0 * 1024.0), 2) + " GB", panel);
                                ulong freeSpaceBytes = (ulong)volumeObj["FreeSpace"];
                                AddOutput("      Free Space: " + Math.Round(freeSpaceBytes / (1024.0 * 1024.0 * 1024.0), 2) + " GB", panel);
                            }
                        }
                        AddOutputSeparator(panel);
                    }
                }
                catch (Exception ex)
                {
                    AddOutput("Error fetching Disk info: " + ex.Message, panel);
                }
            });
        }

                private NetworkInterface[] _networkInterfaces;
        private long[] _lastBytesSent;
        private long[] _lastBytesReceived;

        private void InitializeNetworkCounters()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                _networkInterfaces = Array.Empty<NetworkInterface>();
                _lastBytesSent = Array.Empty<long>();
                _lastBytesReceived = Array.Empty<long>();
                return;
            }

            _networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            _lastBytesSent = new long[_networkInterfaces.Length];
            _lastBytesReceived = new long[_networkInterfaces.Length];

            // Initialize last values
            for (int i = 0; i < _networkInterfaces.Length; i++)
            {
                if (_networkInterfaces[i].Supports(NetworkInterfaceComponent.IPv4))
                {
                    var stats = _networkInterfaces[i].GetIPv4Statistics();
                    _lastBytesSent[i] = stats.BytesSent;
                    _lastBytesReceived[i] = stats.BytesReceived;
                }
            }
        }
        
        private void ShowNetworkInfo_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();
            AddCard("Network Information", (panel) =>
            {
                try
                {
                    foreach (var networkInfo in _systemInfoService.GetNetworkInfo())
                    {
                        AddOutput("Name: " + networkInfo.Name, panel);
                        AddOutput("MAC Address: " + networkInfo.MACAddress, panel);
                        AddOutput("Speed: " + networkInfo.Speed + " bps", panel);
                        AddOutputSeparator(panel);
                    }
                }
                catch (Exception ex)
                {
                    AddOutput("Error fetching Network info: " + ex.Message, panel);
                }
            });
        }

        private void ShowSystemSummary_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();
            AddCard("System Summary", (panel) =>
            {
                try
                {
                    var cpuInfo = _systemInfoService.GetSystemSummaryCpuInfo();
                    AddOutput("CPU: " + cpuInfo.Name + " - " + cpuInfo.NumberOfCores + " Cores, " + cpuInfo.NumberOfLogicalProcessors + " Threads", panel);

                    var ramInfo = _systemInfoService.GetSystemSummaryRamInfo();
                    AddOutput("RAM: " + ramInfo.TotalRam + " GB Total", panel);

                    var gpuInfo = _systemInfoService.GetSystemSummaryGpuInfo();
                    AddOutput("GPU: " + gpuInfo.Name + " - " + gpuInfo.AdapterRAM + " bytes of Memory", panel);

                    var diskInfo = _systemInfoService.GetSystemSummaryDiskInfo();
                    AddOutput("Disk: " + diskInfo.Model + " - " + diskInfo.Size + " bytes Capacity", panel);
                    AddOutputSeparator(panel);
                }
                catch (Exception ex)
                {
                    AddOutput("Error fetching system summary: " + ex.Message, panel);
                }
            });
        }

        private void ShowTemperatureInfo_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();
            AddCard("Temperature Information", (panel) =>
            {
                if (!IsRunAsAdmin())
                {
                    AddOutput("Please run the program as administrator to get temperature data.", panel);
                    return;
                }
                AddOutput("Temperature information is not available (Removed the part that was causing the errors)", panel);
            });
        }

        private void TerminateProgram_Click(object sender, RoutedEventArgs e)
        {
            AddOutput("Exiting program...");
            Environment.Exit(0);
        }

        private void ContinueRun_Click(object sender, RoutedEventArgs e)
        {
            AddOutput("Restarting monitoring...");
            isStopped = false;
        }

        private void StopRun_Click(object sender, RoutedEventArgs e)
        {
            AddOutput("Program terminated.");
            isStopped = true;
        }

        private async void RunBenchmark_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();
            AddCard("Benchmark Results", (panel) => { RunBenchmarkAsync(panel); });
        }

        private async Task RunBenchmarkAsync(StackPanel panel)
        {
            await Task.Run(() =>
            {
                Benchmark benchmark = new Benchmark();
                _model.BenchmarkRun(benchmark);
                Dispatcher.Invoke((Action)(() =>
                {
                    AddOutput("Benchmark Score: " + benchmark.score.ToString("N0"), panel);
                    AddOutput("Threads: " + benchmark.threads, panel);
                    AddOutput("Time: " + benchmark.time.ToString("F2") + " seconds", panel);
                    AddOutputSeparator(panel);
                    AddOutput("Floating Point Score: " + benchmark.floatingPointScore.ToString("N0"), panel);
                    AddOutput("Integer Score: " + benchmark.integerScore.ToString("N0"), panel);
                    AddOutput("Memory Score: " + benchmark.memoryScore.ToString("N0"), panel);
                    AddOutput("Mixed Score: " + benchmark.mixedScore.ToString("N0"), panel);
                    AddOutputSeparator(panel);
                }));
            });
        }
    }
}