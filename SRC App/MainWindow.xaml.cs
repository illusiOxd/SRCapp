using System;
using System.Collections.Generic;
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
//Removed using OpenHardwareMonitor.Hardware;
using System.Management;


namespace SystemResourcesControlWpf
{
    public partial class MainWindow : Window
    {
         private readonly ISystemInfoService _systemInfoService;
         private bool isStopped;
         private readonly Model _model;
        //Removed private OHM.Computer _computer;


        public MainWindow()
        {
            InitializeComponent();
            _systemInfoService = new SystemInfoService();
              _model = new Model(_systemInfoService);
           //Removed  InitializeOpenHardwareMonitor();
        }
       //Removed InitializeOpenHardwareMonitor()
        
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
            if(panel == null)
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
            GroupBox card = new GroupBox { Header = header, Margin = new Thickness(0, 5, 0, 5)};
              StackPanel panel = new StackPanel();
           card.Content = panel;
            content(panel);
           OutputPanel.Children.Add(card);

       }
       private void ShowCpuInfo_Click(object sender, RoutedEventArgs e)
        {
             OutputPanel.Children.Clear();
            AddCard("CPU Information", (panel)=> {
                 DisplayDetailedCpuInfo(panel);
           });
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
            AddCard("GPU Information", (panel) => {
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
           AddCard("RAM Information", (panel) => {
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
            AddCard("Disk Information", (panel) => {
                try
                {
                   foreach (var diskInfo in _systemInfoService.GetDiskInfo())
                    {
                        AddOutput("Model: " + diskInfo.Model, panel);
                       AddOutput("Interface Type: " + diskInfo.InterfaceType, panel);
                        AddOutput("Size: " + diskInfo.Size + " GB", panel);
                       AddOutput("Media Type: " + diskInfo.MediaType, panel);
                       AddOutputSeparator(panel);
                    }
                }
                catch (Exception ex)
                {
                     AddOutput("Error fetching Disk info: " + ex.Message, panel);
                }
            });
         }
        private void ShowNetworkInfo_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();
           AddCard("Network Information", (panel) => {
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
           AddCard("System Summary", (panel) => {
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
            AddCard("Temperature Information", (panel) => {
                if (!IsRunAsAdmin()) {
                   AddOutput("Please run the program as administrator to get temperature data.", panel);
                   return;
                }
                 AddOutput("Temperature information is not available (Removed the part that was causing the errors)", panel);
             });
        }
       //Removed  GetManagementObjectValue()
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
           AddCard("Benchmark Results", (panel) => {
               RunBenchmarkAsync(panel);
             });
         }
         private async Task RunBenchmarkAsync(StackPanel panel)
        {
            await Task.Run(() =>
            {
                  Benchmark benchmark = new Benchmark();
                 _model.BenchmarkRun(benchmark);
                Dispatcher.Invoke(() =>
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
                 });
            });
         }
    }
}