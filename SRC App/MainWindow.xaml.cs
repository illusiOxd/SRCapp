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

namespace SystemResourcesControlWpf
{
    public partial class MainWindow : Window
    {
        private readonly ISystemInfoService _systemInfoService;
        private bool isStopped;
        private readonly Model _model;


        public MainWindow()
        {
            InitializeComponent();
            _systemInfoService = new SystemInfoService();
              _model = new Model(_systemInfoService);
        }

        private void AddOutput(string text)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 2, 0, 2),
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 14
            };
            OutputPanel.Children.Add(textBlock);
        }

        private void AddOutputSeparator()
        {
            var separator = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 0, 1), Margin = new Thickness(0, 5, 0, 5) };
            OutputPanel.Children.Add(separator);
        }
        private void ShowCpuInfo_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();
             AddOutput("----- CPU Information -----");
            DisplayDetailedCpuInfo();
        }

         private void DisplayDetailedCpuInfo()
        {
            try
            {
                var cpuInfo = _systemInfoService.GetDetailedCpuInfo();
                  AddOutput("--- Processor Information ---");
                AddOutput($"Processor Name: {cpuInfo.Name}");
                AddOutput($"Manufacturer: {cpuInfo.Manufacturer}");
                AddOutput($"Number of Cores: {cpuInfo.NumberOfCores}");
                AddOutput($"Number of Logical Processors: {cpuInfo.NumberOfLogicalProcessors}");
                 AddOutput($"Max Clock Speed: {cpuInfo.MaxClockSpeed} MHz");
                AddOutput($"Current Clock Speed: {cpuInfo.CurrentClockSpeed} MHz");
                AddOutput($"Processor ID: {cpuInfo.ProcessorId}");
               AddOutput($"L2 Cache Size: {cpuInfo.L2CacheSize} KB");
                AddOutput($"L3 Cache Size: {cpuInfo.L3CacheSize} KB");
                AddOutput($"Architecture: {cpuInfo.Architecture}");
               AddOutput($"Processor Type: {cpuInfo.ProcessorType}");
                AddOutput($"Status: {cpuInfo.Status}");
                AddOutputSeparator();
            }
            catch (Exception ex)
            {
                AddOutput($"Error fetching CPU info: {ex.Message}");
            }
        }


        private void ShowGpuInfo_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();
            AddOutput("----- GPU Information -----");
            try
            {
                foreach (var gpuInfo in _systemInfoService.GetGpuInfo())
                {
                     AddOutput("--- GPU Information ---");
                    AddOutput($"GPU Name: {gpuInfo.Name}");
                     AddOutput($"Adapter RAM: {gpuInfo.AdapterRAM}");
                   AddOutput($"Driver Version: {gpuInfo.DriverVersion}");
                    AddOutput($"Video Processor: {gpuInfo.VideoProcessor}");
                    AddOutputSeparator();
                }
            }
            catch (Exception ex)
            {
               AddOutput($"Error fetching GPU info: {ex.Message}");
            }
        }
        private void ShowRamInfo_Click(object sender, RoutedEventArgs e)
        {
             OutputPanel.Children.Clear();
             AddOutput("----- RAM Information -----");
            try
            {
                var ramInfos = _systemInfoService.GetRamInfo();
                AddOutput($"Total Installed RAM: {ramInfos.TotalCapacity} GB\n");

                int moduleNumber = 1;
                foreach (var ramInfo in ramInfos.Modules)
                {
                    AddOutput($"Module {moduleNumber++}:");
                     AddOutput($"  Capacity: {ramInfo.Capacity} GB");
                    AddOutput($"  Manufacturer: {ramInfo.Manufacturer}");
                    AddOutput($"  Speed: {ramInfo.Speed} MHz");
                   AddOutput($"  Part Number: {ramInfo.PartNumber}\n");
                   AddOutputSeparator();
                }
            }
            catch (Exception ex)
            {
                AddOutput($"Error fetching RAM info: {ex.Message}");
            }
        }
         private void ShowDiskInfo_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();
            AddOutput("----- Disk Information -----");
            try
            {
                foreach (var diskInfo in _systemInfoService.GetDiskInfo())
                {
                     AddOutput($"Model: {diskInfo.Model}");
                     AddOutput($"Interface Type: {diskInfo.InterfaceType}");
                    AddOutput($"Size: {diskInfo.Size} GB");
                     AddOutput($"Media Type: {diskInfo.MediaType}");
                    AddOutputSeparator();
                 }
            }
            catch (Exception ex)
            {
                 AddOutput($"Error fetching Disk info: {ex.Message}");
            }
        }
        private void ShowNetworkInfo_Click(object sender, RoutedEventArgs e)
        {
           OutputPanel.Children.Clear();
            AddOutput("----- Network Information -----");
            try
            {
                foreach (var networkInfo in _systemInfoService.GetNetworkInfo())
                {
                    AddOutput($"Name: {networkInfo.Name}");
                    AddOutput($"MAC Address: {networkInfo.MACAddress}");
                   AddOutput($"Speed: {networkInfo.Speed} bps");
                  AddOutputSeparator();
                }
            }
            catch (Exception ex)
            {
                AddOutput($"Error fetching Network info: {ex.Message}");
            }
        }
         private void ShowSystemSummary_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();
             AddOutput("----- System Summary -----");
            try
            {

                var cpuInfo = _systemInfoService.GetSystemSummaryCpuInfo();
                 AddOutput($"CPU: {cpuInfo.Name} - {cpuInfo.NumberOfCores} Cores, {cpuInfo.NumberOfLogicalProcessors} Threads");

                var ramInfo = _systemInfoService.GetSystemSummaryRamInfo();
                 AddOutput($"RAM: {ramInfo.TotalRam} GB Total");

                var gpuInfo = _systemInfoService.GetSystemSummaryGpuInfo();
                AddOutput($"GPU: {gpuInfo.Name} - {gpuInfo.AdapterRAM} bytes of Memory");

                var diskInfo = _systemInfoService.GetSystemSummaryDiskInfo();
                 AddOutput($"Disk: {diskInfo.Model} - {diskInfo.Size} bytes Capacity");
                 AddOutputSeparator();
            }
            catch (Exception ex)
            {
                AddOutput($"Error fetching system summary: {ex.Message}");
            }
        }
        private void ShowTemperatureInfo_Click(object sender, RoutedEventArgs e)
        {
            OutputPanel.Children.Clear();
            AddOutput("----- Temperature Information -----");
            try
            {
                var temperatureInfo = _systemInfoService.GetTemperatureInfo();
                AddOutput(temperatureInfo.HasTemperatureData
                     ? $"Temperature: {temperatureInfo.TemperatureCelsius:F2}°C"
                    : "No temperature information available.");
                  AddOutputSeparator();
            }
            catch (Exception ex)
            {
                 AddOutput($"Error fetching Temperature info: {ex.Message}");
            }
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
             BenchmarkProgressBar.Visibility = Visibility.Visible;
             BenchmarkStatusTextBlock.Visibility = Visibility.Visible;
             BenchmarkStageTextBlock.Visibility = Visibility.Visible;

             BenchmarkStatusTextBlock.Text = "Initializing...";
            BenchmarkProgressBar.Value = 0;
            await Task.Run(() =>
            {
                Benchmark benchmark = new Benchmark();
                 _model.BenchmarkRun(benchmark);

                 Dispatcher.Invoke(() =>
                {
                   BenchmarkStatusTextBlock.Text = "Running Benchmark...";
                    BenchmarkStageTextBlock.Text = "Running Floating point operation...";
                });
                for (int i = 0; i <= 100; i+= 25) {
                     System.Threading.Thread.Sleep(100);
                    Dispatcher.Invoke(() => {
                        BenchmarkProgressBar.Value = i;
                       if (i == 25)  BenchmarkStageTextBlock.Text = "Running Integer operation...";
                       else if (i == 50)  BenchmarkStageTextBlock.Text = "Running memory operation...";
                       else if (i == 75)  BenchmarkStageTextBlock.Text = "Running mixed operation...";
                       else if (i == 100) BenchmarkStageTextBlock.Text = "Finishing...";
                    });
               }


                Dispatcher.Invoke(() =>
                {
                    BenchmarkStatusTextBlock.Text = "Completing...";
                     AddOutput($"Benchmark Score: {benchmark.score:N0}");
                     AddOutput($"Threads: {benchmark.threads}");
                     AddOutput($"Time: {benchmark.time:F2} seconds");
                    AddOutputSeparator();
                     AddOutput($"Floating Point Score: {benchmark.floatingPointScore:N0}");
                     AddOutput($"Integer Score: {benchmark.integerScore:N0}");
                   AddOutput($"Memory Score: {benchmark.memoryScore:N0}");
                    AddOutput($"Mixed Score: {benchmark.mixedScore:N0}");
                  AddOutputSeparator();
                    BenchmarkStatusTextBlock.Visibility = Visibility.Collapsed;
                    BenchmarkProgressBar.Visibility = Visibility.Collapsed;
                      BenchmarkStageTextBlock.Visibility = Visibility.Collapsed;
                   });
            });
        }
    }
}