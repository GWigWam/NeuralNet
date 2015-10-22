using NeuralNet.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.Connections {

    [DebuggerDisplay("Connection {FromNode.Name} --> {ToNode.Name}")]
    public class Connection {

        public float Weight {
            get; set;
        }

        public Node FromNode {
            get;
        }

        public Node ToNode {
            get;
        }

        public float Output => Weight * FromNode.Output;

        private Connection(float weight, Node fromNode, Node toNode) {
            Weight = weight;
            FromNode = fromNode;
            ToNode = toNode;
        }

        public static Connection Create(float weight, Node fromNode, Node toNode) {
            var con = new Connection(weight, fromNode, toNode);

            fromNode.AddOutgoingConnection(con);
            toNode.AddIncommingConnection(con);

            return con;
        }
    }
}