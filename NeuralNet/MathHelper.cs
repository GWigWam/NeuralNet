using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet {

    public static class MathHelper {

        public static float Normalize(float input, float min, float max) {
            var val = (input - min) / (max - min);
            return val;
        }

        public static IEnumerable<float> Normalize(IEnumerable<float> collection) {
            float min = collection.Min();
            float max = collection.Max();

            return collection.Select(f => Normalize(f, min, max));
        }
    }
}