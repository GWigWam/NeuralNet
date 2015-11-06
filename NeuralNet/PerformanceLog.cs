using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeuralNet {

    public static class PerformanceLog {
        private static ConcurrentDictionary<int, long> ThreadLastLog;

        public static ConcurrentDictionary<string, long> Processes;

        private static long LastLog => ThreadLastLog[Thread.CurrentThread.ManagedThreadId];

        static PerformanceLog() {
            Processes = new ConcurrentDictionary<string, long>();
            ThreadLastLog = new ConcurrentDictionary<int, long>();
            ResetCurTimer();
        }

        public static void ResetCurTimer() {
            ThreadLastLog.AddOrUpdate(Thread.CurrentThread.ManagedThreadId, Environment.TickCount, (i, l) => Environment.TickCount);
        }

        public static void LogSingle(string s) {
            Console.WriteLine($"{Environment.TickCount - LastLog,-5}Ms | {s}");
            ResetCurTimer();
        }

        public static void LogProcess(string name) {
            /*if(Processes.ContainsKey(name)) {
                Processes[name] += (Environment.TickCount - LastLog);
            } else {
                Processes.TryAdd(name, Environment.TickCount - LastLog);
            }*/

            var duration = Environment.TickCount - LastLog;

            Processes.AddOrUpdate(name, duration, (s, l) => l + duration);

            ResetCurTimer();
        }

        public static void PrintProcesses(bool reset) {
            var orgCol = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"Processes: ({Processes.Sum(kvp => kvp.Value)}Ms)");
            foreach(var process in Processes) {
                Console.WriteLine($"- {process.Value,6}Ms : {process.Key}");
            }
            Console.ForegroundColor = orgCol;

            if(reset) {
                ResetProcesses();
            }
        }

        public static void ResetProcesses() {
            Processes = new ConcurrentDictionary<string, long>();
        }
    }
}