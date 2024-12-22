using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
         public void BenchmarkRun(Benchmark benchmark)
        {
             int threadCount = Environment.ProcessorCount;
            Dictionary<int, (double, double, double, double, double, double, double, double, double)> threadScores = new Dictionary<int, (double, double, double, double, double, double, double, double, double)>();
             Stopwatch stopwatch = Stopwatch.StartNew();
            double totalScore = 0;
            stopwatch.Start();
             Parallel.For(0, threadCount, i => threadScores[i] = SimulateHeavyWorkload(i, benchmark));

            stopwatch.Stop();

             double floatingPointTotalScore = 0;
             double integerTotalScore = 0;
            double memoryTotalScore = 0;
            double mixedTotalScore = 0;

             for (int i = 0; i < threadCount; i++)
            {
                var (threadScore, floatingPointScore, integerScore, memoryScore, mixedScore, floatingPointTime, integerTime, memoryTime, mixedTime) = threadScores[i];
               
                totalScore += CalculateScore(threadScore, stopwatch.Elapsed);
                 floatingPointTotalScore += floatingPointScore;
                 integerTotalScore += integerScore;
                 memoryTotalScore += memoryScore;
                 mixedTotalScore += mixedScore;
                
             }

            benchmark.score = totalScore;
            benchmark.threads = threadCount;
            benchmark.time = stopwatch.Elapsed.TotalSeconds;
               benchmark.floatingPointScore = floatingPointTotalScore;
             benchmark.integerScore = integerTotalScore;
            benchmark.memoryScore = memoryTotalScore;
            benchmark.mixedScore = mixedTotalScore;
          
         }


        public void InitializeThreadCounters() =>
            ThreadCounters = Enumerable.Range(0, Environment.ProcessorCount)
                .Select(i => new PerformanceCounter("Processor", "% Processor Time", i.ToString())).ToList();


       public (double, double, double, double, double, double, double, double, double) SimulateHeavyWorkload(int threadIndex, Benchmark benchmark)
        {
            double result = 0;
            double floatingPointResult = 0;
            double integerResult = 0;
            double memoryResult = 0;
            double mixedResult = 0;

            double floatingPointTime = 0;
            double integerTime = 0;
           double memoryTime = 0;
           double mixedTime = 0;

            const int iterations = 10_000_000;
           Random random = new Random(threadIndex);

             for (int i = 1; i <= iterations; i++)
            {
             int operationType = random.Next(4);
               Stopwatch operationStopwatch = Stopwatch.StartNew();

               switch (operationType)
               {
                  case 0: // Floating-point operations
                       floatingPointResult += Math.Sqrt(i) * Math.Sin(i);
                       break;
                     case 1: // Integer operations
                       integerResult += i * (i % 3 == 0 ? -1 : 1);
                         break;
                    case 2: // Memory operations
                         int arraySize = random.Next(1000, 10000);
                         byte[] array = new byte[arraySize];
                        for (int j = 0; j < arraySize; j++)
                       {
                           array[j] = (byte)(i % 256);
                       }
                        memoryResult += array[random.Next(arraySize)];
                      break;
                   case 3: // Mixed operations
                      mixedResult += Math.Abs(Math.Sqrt(i) * Math.Log(i + 1)) * (i % 2 == 0 ? -1 : 1);
                      break;
                }
               operationStopwatch.Stop();

               switch (operationType)
               {
                   case 0:
                      floatingPointTime += operationStopwatch.Elapsed.TotalSeconds;
                    break;
                 case 1:
                       integerTime += operationStopwatch.Elapsed.TotalSeconds;
                   break;
                  case 2:
                      memoryTime += operationStopwatch.Elapsed.TotalSeconds;
                     break;
                   case 3:
                      mixedTime += operationStopwatch.Elapsed.TotalSeconds;
                     break;
                }
            }
            result = floatingPointResult + integerResult + memoryResult + mixedResult;

            double threadScore = result / 1_000_000;

            return (threadScore, floatingPointResult, integerResult, memoryResult, mixedResult, floatingPointTime, integerTime, memoryTime, mixedTime);
        }


        public int CalculateScore(double threadScore, TimeSpan elapsedTime)
        {
            double baseScore = 10000.0 / (1 + threadScore / 1000.0);
            double timeBonus = 10000.0 / (elapsedTime.TotalMilliseconds + 1);
           double totalScore = baseScore + timeBonus;
           
           return Math.Max(0, (int)totalScore);
         }
    }
}