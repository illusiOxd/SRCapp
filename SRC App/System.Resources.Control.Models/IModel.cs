using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Resources.Control.Models
{
    public interface IModel
    {
        List<PerformanceCounter> ThreadCounters { get; set; }
        void InitializeThreadCounters();
        (double, double, double, double, double, double, double, double, double) SimulateHeavyWorkload(int threadIndex, Benchmark benchmark);
        int CalculateScore(double threadScore, TimeSpan elapsedTime);
        void BenchmarkRun(Benchmark benchmark);
    }
}