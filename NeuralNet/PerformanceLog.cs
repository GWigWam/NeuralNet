using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet {

    public static class PerformanceLog {
        private static long lastLog;

        static PerformanceLog() {
            lastLog = Environment.TickCount;
        }

        public static void Log(string s) {
            var now = Environment.TickCount;
            var timeDif = now - lastLog;
            Console.WriteLine($"{timeDif}Ms | {s}");

            lastLog = now;
        }
    }
}