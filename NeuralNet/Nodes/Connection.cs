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

        public double Weight {
            get; set;
        }

        public Node FromNode {
            get;
        }

        public Node ToNode {
            get;
        }

        public double Output => Weight * FromNode.Output;

        private Connection(double weight, Node fromNode, Node toNode) {
            Weight = weight;
            FromNode = fromNode;
            ToNode = toNode;
        }

        public void Delete() {
            Delete(this);
        }

        public static Connection Create(double weight, Node fromNode, Node toNode) {
            var con = new Connection(weight, fromNode, toNode);

            fromNode.AddOutgoingConnection(con);
            toNode.AddIncommingConnection(con);

            return con;
        }

        public static void Delete(Connection connection) {
            connection.FromNode.RemoveConnection(connection);
            connection.ToNode.RemoveConnection(connection);
        }
    }
}