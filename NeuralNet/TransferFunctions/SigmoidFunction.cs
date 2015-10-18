using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.TransferFunctions {

    internal class SigmoidFunction : TransferFunction {

        public override float Calculate(IEnumerable<float> input) {
            var val = Sigmoid(input.Sum());
            return (float)val;
        }

        private double Sigmoid(float input) {
            var val = (1 / (1 + Math.Pow(Math.E, -input)));
            return val;
        }
    }
}