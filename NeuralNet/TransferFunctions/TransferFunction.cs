using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.TransferFunctions {

    public abstract class TransferFunction {

        public double Calculate(params double[] input) => Calculate((IEnumerable<double>)input);

        public double Calculate(IEnumerable<double> input) => Calculate(input.Sum());

        public abstract double Calculate(double input);

        public abstract double Derivative(double input);
    }
}