using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.TransferFunctions {

    public class SigmoidFunction : TransferFunction {

        public override float Calculate(IEnumerable<float> input) {
            var val = Sigmoid(input.Sum());
            return (float)val;
        }

        private double Sigmoid(float input) {
            var val = (1f / (1f + Math.Pow(Math.E, -input)));
            return val;
        }

        public override float Derivative(float input) {
            var val = input * (1f - input);
            return val;
        }
    }
}