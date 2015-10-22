using NeuralNet.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.Connections {

    [DebuggerDisplay("WeightedConnection out: {Output}")]
    public class WeightedConnection : Connection {

        public float Weight {
            get; set;
        }

        public INode InputNode {
            get;
        }

        public float Output => Weight * InputNode.Output;

        public WeightedConnection(float weight, INode inputNode) {
            InputNode = inputNode;
            Weight = weight;
        }
    }
}