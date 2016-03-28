using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet {

    public static class MathHelper {

        // chosen by fair dice roll
        // guaranteed to be random
        private const int Seed = 4;

        private static Random Rand = new Random(Seed);

        public static double Normalize(double input, double min, double max) {
            var val = (input - min) / (max - min);
            return val;
        }

        public static IEnumerable<double> Normalize(IEnumerable<double> collection) {
            double min = collection.Min();
            double max = collection.Max();

            return collection.Select(f => Normalize(f, min, max));
        }

        public static double ShiftRange(double value, double orgMin, double orgMax, double newMin, double newMax) {
            var val = (((value - orgMin) / (orgMax - orgMin)) * (newMax - newMin)) + newMin;
            return val;
        }

        public static double GuassianRandom(double stdDev, double mean) {
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(Rand.NextDouble())) * Math.Sin(2.0 * Math.PI * Rand.NextDouble()); //random normal(0,1)
            double randNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

            return randNormal;
        }
    }
}