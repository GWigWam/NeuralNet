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
    public class Perceptron : INode {
        private float? CachedOutput;

        public string Name {
            get;
        }

        public TransferFunction TransferFunction {
            get;
        }

        public Connection[] Input {
            get;
        }

        public float Output => CachedOutput ?? (float)(CachedOutput = CalculateOutput());

        public Perceptron(TransferFunction transferFunction, IEnumerable<Connection> connections, string name = "X") {
            TransferFunction = transferFunction;
            Input = connections.ToArray();
            Name = name;
        }

        public Perceptron(TransferFunction transferFunction, Connection singleConnection, string name = "X") : this(transferFunction, new Connection[] { singleConnection }, name) {
        }

        public void ResetCache() {
            CachedOutput = null;
        }

        private float CalculateOutput() {
            var inputs = Input.Select(c => c.Output);
            var output = TransferFunction.Calculate(inputs);
            return output;
        }
    }
}