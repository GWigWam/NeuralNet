using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.TransferFunctions {

    public class SigmoidFunction : TransferFunction {
        public override float ExtremeMax => 1;

        public override float ExtremeMin => 0;

        public override float Calculate(float input) {
            var val = (1.0 / (1.0 + Math.Pow(Math.E, -input)));
            return (float)val;
        }

        public override float Derivative(float input) {
            var val = input * (1.0 - input);
            return (float)val;
        }
    }
}