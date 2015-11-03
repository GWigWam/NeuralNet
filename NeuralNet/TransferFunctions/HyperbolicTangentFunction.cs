using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.TransferFunctions {

    public class HyperbolicTangentFunction : TransferFunction {

        public override double Calculate(double input) {
            var val = Math.Tanh(input);
            return val;
        }

        public override double Derivative(double input) {
            var val = 1.0 - Math.Pow(Math.Tanh(input), 2);
            return val;
        }
    }
}