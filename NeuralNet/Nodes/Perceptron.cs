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
        private float? CachedOutput;

        public TransferFunction TransferFunction {
            get;
        }

        public override float Output => CachedOutput ?? (float)(CachedOutput = CalculateOutput());

        public Perceptron(TransferFunction transferFunction, string name = "X") : base(name) {
            TransferFunction = transferFunction;
        }

        public void ResetCache() {
            CachedOutput = null;
        }

        private float CalculateOutput() {
            var inputs = GetIncommingConnections().Select(c => c.Output);
            var output = TransferFunction.Calculate(inputs);
            return output;
        }
    }
}