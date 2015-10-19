using NeuralNet.Connections;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet {

    [DebuggerDisplay("Perceptron out: {Output}")]
    public class Perceptron {
        private float? CachedOutput;

        public TransferFunction Transfer {
            get;
        }

        public Connection[] Connections {
            get;
        }

        public float Output => CachedOutput ?? (float)(CachedOutput = CalculateOutput());

        public Perceptron(TransferFunction transferFunction, IEnumerable<Connection> connections) {
            Transfer = transferFunction;
            Connections = connections.ToArray();
        }

        public Perceptron(TransferFunction transferFunction, Connection singleConnection) : this(transferFunction, new Connection[] { singleConnection }) {
        }

        public void ResetCache() {
            CachedOutput = null;
        }

        private float CalculateOutput() {
            var inputs = Connections.Select(c => c.Output);
            var output = Transfer.Calculate(inputs);
            return output;
        }
    }
}