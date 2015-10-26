using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.TransferFunctions {

    public class SigmoidFunction : TransferFunction {

        public override double Calculate(IEnumerable<double> input) {
            var val = Sigmoid(input.Sum());
            return val;
        }

        private double Sigmoid(double input) {
            var val = (1.0 / (1.0 + Math.Pow(Math.E, -input)));
            return val;
        }

        public override double Derivative(double input) {
            var val = input * (1.0 - input);
            return val;
        }
    }
}