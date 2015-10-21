using NeuralNet.Connections;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet {

    [DebuggerDisplay("Perceptron '{Name}' = [{Output}]")]
    public class Perceptron {
        private float? CachedOutput;

        public string Name {
            get;
        }

        public TransferFunction TransferFunction {
            get;
        }

        public Connection[] Connections {
            get;
        }

        public float Output => CachedOutput ?? (float)(CachedOutput = CalculateOutput());

        public Perceptron(TransferFunction transferFunction, IEnumerable<Connection> connections, string name = "X") {
            TransferFunction = transferFunction;
            Connections = connections.ToArray();
            Name = name;
        }

        public Perceptron(TransferFunction transferFunction, Connection singleConnection, string name = "X") : this(transferFunction, new Connection[] { singleConnection }, name) {
        }

        public void ResetCache() {
            CachedOutput = null;
        }

        private float CalculateOutput() {
            var inputs = Connections.Select(c => c.Output);
            var output = TransferFunction.Calculate(inputs);
            return output;
        }
    }
}