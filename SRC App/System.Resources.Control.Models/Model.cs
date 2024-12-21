using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace System.Resources.Control.Models
{
    public class Model : IModel
    {
        private readonly Services.ISystemInfoService _systemInfoService;
        public List<PerformanceCounter> ThreadCounters { get; set; }

        public Model(Services.ISystemInfoService systemInfoService)
        {
            _systemInfoService = systemInfoService;
            InitializeThreadCounters();
        }


        public void InitializeThreadCounters() =>
            ThreadCounters = Enumerable.Range(0, Environment.ProcessorCount)
                .Select(i => new PerformanceCounter("Processor", "% Processor Time", i.ToString())).ToList();


        public double SimulateHeavyWorkload(int threadIndex)
        {
            double result = 0;
            for (int i = 1; i <= 100_000_000; i++)
                result += Math.Sqrt(i) * Math.Sin(i);
            return result / 1_000_000;
        }


        public int CalculateScore(double threadScore, TimeSpan elapsedTime) =>
            Math.Max(0, (int)(10000.0 / (1 + threadScore / 1000.0) + 10000.0 / (elapsedTime.TotalMilliseconds + 1)));
    }
}