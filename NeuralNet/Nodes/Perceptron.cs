using NeuralNet.Connections;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.Nodes {

    [DebuggerDisplay("Perceptron '{Name}' = [{Output}]")]
    public class Perceptron : Node {
        private double? CachedOutput;

        public TransferFunction TransferFunction {
            get;
        }

        public override double Output => CachedOutput ?? (double)(CachedOutput = CalculateOutput());

        public Perceptron(TransferFunction transferFunction, string name = "X") : base(name) {
            TransferFunction = transferFunction;
        }

        public void ResetCache() {
            CachedOutput = null;
        }

        private double CalculateOutput() {
            var inputs = GetIncommingConnections().Select(c => c.Output);
            var output = TransferFunction.Calculate(inputs);
            return output;
        }
    }
}