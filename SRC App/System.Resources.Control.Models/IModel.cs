using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Resources.Control.Models
{
    public interface IModel
    {
        List<PerformanceCounter> ThreadCounters { get; set; }
        void InitializeThreadCounters();
        double SimulateHeavyWorkload(int threadIndex);
        int CalculateScore(double threadScore, TimeSpan elapsedTime);
    }
}