using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeuralNet {

    [DebuggerStepThrough]
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

        public static void LogSingle(string msg, bool logTime, ConsoleColor? fontColor = null, ConsoleColor? backColor = null) {
            var dutation = Environment.TickCount - LastLog;

            var startFontColor = Console.ForegroundColor;
            var startBackColor = Console.BackgroundColor;
            Console.ForegroundColor = fontColor ?? startFontColor;
            Console.BackgroundColor = backColor ?? startBackColor;

            if(logTime) {
                Console.WriteLine($"{dutation,-5}Ms | {msg}");
                ResetCurTimer();
            } else {
                Console.WriteLine(msg);
            }

            Console.ForegroundColor = startFontColor;
            Console.BackgroundColor = startBackColor;
        }

        public static void LogProcess(string name) {
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