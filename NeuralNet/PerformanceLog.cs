using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet {

    public static class PerformanceLog {
        private static long lastLog;

        private static Dictionary<string, long> Processes;

        static PerformanceLog() {
            ResetCurTimer();
            Processes = new Dictionary<string, long>();
        }

        public static void ResetCurTimer() {
            lastLog = Environment.TickCount;
        }

        public static void LogSingle(string s) {
            Console.WriteLine($"{Environment.TickCount - lastLog,-5}Ms | {s}");
            ResetCurTimer();
        }

        public static void LogProcess(string name) {
            if(Processes.ContainsKey(name)) {
                Processes[name] += (Environment.TickCount - lastLog);
            } else {
                Processes.Add(name, Environment.TickCount - lastLog);
            }
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
            Processes = new Dictionary<string, long>();
        }
    }
}