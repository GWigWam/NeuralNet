using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet {

    public static class MathHelper {

        public static double Normalize(double input, double min, double max) {
            var val = (input - min) / (max - min);
            return val;
        }

        public static IEnumerable<double> Normalize(IEnumerable<double> collection) {
            double min = collection.Min();
            double max = collection.Max();

            return collection.Select(f => Normalize(f, min, max));
        }
    }
}