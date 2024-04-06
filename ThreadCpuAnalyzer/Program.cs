using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThreadState = System.Threading.ThreadState;

namespace ThreadCpuAnalyzer
{
    public static class ThreadCpuAnalyzer
    {

        private static ConcurrentDictionary<long, Thread> Threads = new ConcurrentDictionary<long, Thread>();
        private static ConcurrentDictionary<long, List<double>> Reports = new ConcurrentDictionary<long, List<double>>();
        static void Main(string[] args)
        {
            var Thread1 = new Thread(() =>
            {
                while (true)
                {
                    for (int i = 0; i < int.MaxValue / 1000; i++)
                    {
                        var x = i ^ 2;
                    }
                    Thread.Sleep(1);
                }
            });

            var Thread2 = new Thread(() =>
            {
                while (true)
                {
                    for (int i = 0; i < int.MaxValue / 1000; i++)
                    {
                        var x = i ^ 3;
                    }
                    Thread.Sleep(2);
                }
            });

            var Thread3 = new Thread(() =>
            {
                while (true)
                {
                    for (int i = 0; i < int.MaxValue / 1000; i++)
                    {
                        var x = i ^ 4;
                    }
                    Thread.Sleep(3);
                }
            });

            var Thread4 = new Thread(() =>
            {
                while (true)
                {
                    for (int i = 0; i < int.MaxValue / 1000; i++)
                    {
                        var x = i ^ 5;
                    }
                    Thread.Sleep(4);
                }
            });

            var Thread5 = new Thread(() =>
            {
                while (true)
                {
                    for (int i = 0; i < int.MaxValue / 1000; i++)
                    {
                        var x = i ^ 6;
                    }
                    Thread.Sleep(5);
                }
            });

            var Thread6 = new Thread(() =>
            {
                while (true)
                {
                    for (int i = 0; i < int.MaxValue / 1000; i++)
                    {
                        var x = i ^ 7;
                    }
                    Thread.Sleep(6);
                }
            });
            var Thread7 = new Thread(() =>
            {
                while (true)
                {
                    for (int i = 0; i < int.MaxValue / 1000; i++)
                    {
                        var x = i ^ 8;
                    }
                    Thread.Sleep(7);
                }
            });
            var Thread8 = new Thread(() =>
            {
                while (true)
                {
                    for (int i = 0; i < int.MaxValue / 1000; i++)
                    {
                        var x = i ^ 9;
                    }
                    Thread.Sleep(8);
                }
            });
            var Thread9 = new Thread(() =>
            {
                while (true)
                {
                    for (int i = 0; i < int.MaxValue / 1000; i++)
                    {
                        var x = i ^ 10;
                    }
                    Thread.Sleep(9);
                }
            });
            var Thread10 = new Thread(() =>
            {
                while (true)
                {
                    for (int i = 0; i < int.MaxValue / 1000; i++)
                    {
                        var x = i ^ 11;
                    }
                    Thread.Sleep(10);
                }
            });



            AddStartThread(ref Thread10, "Thread10");
            AddStartThread(ref Thread9, "Thread9");
            AddStartThread(ref Thread8, "Thread8");
            AddStartThread(ref Thread7, "Thread7");
            AddStartThread(ref Thread6, "Thread6");
            AddStartThread(ref Thread5, "Thread5");
            AddStartThread(ref Thread4, "Thread4");
            AddStartThread(ref Thread3, "Thread3");
            AddStartThread(ref Thread2, "Thread2");
            AddStartThread(ref Thread1, "Thread1");

            StartMonitoring(200, 100, 1000);

            while (true)
            {
                var Rep = GenerateReport();

                foreach (var item in Rep)
                {
                    Console.WriteLine($"{item.Name} -> {item.CpuTimeRatio:n3}%");
                }
                Console.WriteLine(  );
                Thread.Sleep(1000);
            }


        }


        private static bool KeepMonitoring = true;
        public static void StartMonitoring(int SnapshotDuration = 1000, int SnapshotInterval = 1000, int MaxSamples = 100)
        {
            new Thread(() =>
            {
                while (KeepMonitoring)
                {
                    try
                    {

                        var process = Process.GetCurrentProcess();

                        // Capture initial CPU times
                        var initialCpuUsages = new TimeSpan[process.Threads.Count];
                        int i = 0;
                        foreach (ProcessThread thread in process.Threads)
                        {
                            if (Threads.ContainsKey(thread.Id))
                            {
                                initialCpuUsages[i++] = thread.TotalProcessorTime;
                            }
                        }

                        // Wait for a period (e.g., 1 second)
                        Thread.Sleep(SnapshotDuration);

                        // Recalculate CPU usage
                        i = 0;
                        foreach (ProcessThread thread in process.Threads)
                        {
                            if (Threads.ContainsKey(thread.Id))
                            {
                                var managedThread = Threads[thread.Id];
                                var Name = "NA";
                                if (managedThread != null)
                                    Name = managedThread.Name;

                                TimeSpan currentCpuUsage = thread.TotalProcessorTime;
                                TimeSpan cpuUsed = currentCpuUsage - initialCpuUsages[i++];
                                if (!Reports.ContainsKey(thread.Id) || Reports[thread.Id] == null)
                                    Reports[thread.Id] = new List<double>();
                                 Reports[thread.Id].Add(Math.Abs((cpuUsed.TotalMilliseconds + double.Epsilon) / (SnapshotDuration + double.Epsilon)) * 100);

                                while (Reports[thread.Id].Count > MaxSamples)
                                    Reports[thread.Id].RemoveAt(0);
                            }
                        }
                    }
                    catch { }
                    Thread.Sleep(SnapshotInterval);
                }
            })
            { IsBackground = true }.Start();
        }


        public static List<ThreadReport> GenerateReport()
        {
            var AllKeys = Threads.Keys;
            var ThreadReports = new List<ThreadReport>();
            try
            {
                foreach (var key in AllKeys)
                {
                    if (Threads[key].ThreadState == ThreadState.Stopped || Threads[key].ThreadState == ThreadState.Aborted)
                    {
                        Threads.TryRemove(key, out _);
                    }
                }
                AllKeys = Threads.Keys;

                foreach (var key in AllKeys)
                {
                    if (Reports.ContainsKey(key))
                    {
                        var cRep = new ThreadReport()
                        {
                            Id = key,
                            Name = Threads[key].Name,
                            CpuTimeRatio = Reports[key].Any() ? Reports[key].Average() : 0
                        };


                        ThreadReports.Add(cRep);
                    }
                }
            }
            catch { }
            return ThreadReports.OrderByDescending(x => x.CpuTimeRatio).ToList();
        }



        public static void RemoveThread(ref Thread Thread)
        {
            Threads.TryRemove(Thread.ManagedThreadId, out Thread);
        }
        public static void RemoveThread(int ThreadId)
        {
            Threads.TryRemove(ThreadId, out _);
        }
        public static Thread AddStartThread(Thread _thread, string Name = "")
        {
            return AddStartThread(ref _thread, Name, true);
        }

        public static Thread AddStartThread(ref Thread _thread, string Name = "", bool IsBypass = false)
        {
            try
            {
                StackTrace stackTrace = new StackTrace();
                StackFrame stackFrame = stackTrace.GetFrame(IsBypass ? 2 : 1);

                var method = stackFrame.GetMethod();
                var declaringType = method.DeclaringType;
                var methodName = method.Name;
                var namespaceName = declaringType.Namespace;
                int line = stackFrame.GetFileLineNumber();


                _thread.Name = $"[{Name}] {namespaceName}\\{declaringType.FullName}\\{methodName}()\\L:{line}";
                _thread.Start();
                var approxValue = DateTime.Now.Ticks;

                var process = Process.GetCurrentProcess();

                var accurateList = new List<long>();
                foreach (ProcessThread t in process.Threads)
                {
                    accurateList.Add(t.StartTime.Ticks);
                }

                long closestValue = accurateList[0];
                long smallestDifference = Math.Abs(approxValue - accurateList[0]);

                foreach (var accurateValue in accurateList)
                {
                    long currentDifference = Math.Abs(approxValue - accurateValue);

                    if (currentDifference < smallestDifference)
                    {
                        smallestDifference = currentDifference;
                        closestValue = accurateValue;
                    }
                }
                var accurateThreadId = 0;
                foreach (ProcessThread t in process.Threads)
                {
                    if (closestValue == t.StartTime.Ticks)
                    {
                        accurateThreadId = t.Id;
                        break;
                    }
                }

                Threads.TryAdd(accurateThreadId, _thread);
            }
            catch { }

            Thread.Sleep(111);
            return _thread;
        }

        public class ThreadReport
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public double CpuTimeRatio { get; set; }
        }
    }
}
