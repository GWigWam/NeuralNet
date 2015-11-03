﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.TransferFunctions {

    public class SigmoidFunction : TransferFunction {
        public override double ExtremeMax => 1;

        public override double ExtremeMin => 0;

        public override double Calculate(double input) {
            var val = (1.0 / (1.0 + Math.Pow(Math.E, -input)));
            return val;
        }

        public override double Derivative(double input) {
            var val = input * (1.0 - input);
            return val;
        }
    }
}