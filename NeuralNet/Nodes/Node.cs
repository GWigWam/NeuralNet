using NeuralNet.Connections;
using System;
using System.Collections.Generic;

namespace NeuralNet.Nodes {

    public abstract class Node {
        private List<Connection> IncommingConnections;

        private List<Connection> OutgoingConnections;

        public Node(string name) {
            Name = name;

            IncommingConnections = new List<Connection>();
            OutgoingConnections = new List<Connection>();
        }

        public string Name {
            get;
        }

        public abstract double Output {
            get;
        }

        public Connection[] GetIncommingConnections() {
            return IncommingConnections.ToArray();
        }

        public Connection[] GetOutgoingConnections() {
            return OutgoingConnections.ToArray();
        }

        internal void AddIncommingConnection(Connection connection) {
            if(connection.ToNode == this && connection.FromNode != null) {
                IncommingConnections.Add(connection);
            } else {
                throw new ArgumentException("Invalid connection");
            }
        }

        internal void AddOutgoingConnection(Connection connection) {
            if(connection.FromNode == this && connection.ToNode != null) {
                OutgoingConnections.Add(connection);
            } else {
                throw new ArgumentException("Invalid connection");
            }
        }

        internal void RemoveConnection(Connection connection) {
            if(connection.FromNode == this) {
                OutgoingConnections.Remove(connection);
            } else if(connection.ToNode == this) {
                IncommingConnections.Remove(connection);
            }
        }
    }
}