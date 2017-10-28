using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.TransferFunctions {

    public class HyperbolicTangentFunction : TransferFunction {
        public override float ExtremeMax => 1;

        public override float ExtremeMin => -1;

        public override float Calculate(float input) {
            var val = Math.Tanh(input);
            return (float)val;
        }

        public override float Derivative(float input) {
            var val = 1.0 - Math.Pow(Math.Tanh(input), 2);
            return (float)val;
        }
    }
}