using NeuralNet.Connections;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet {

    internal class Perceptron {
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