using NeuralNet.Connections;
using NeuralNet.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NeuralNet.PerformanceLog;

namespace NeuralNet.BackpropagationTraining {

    public class Backpropagate {

        public double LearningRate {
            get; set;
        }

        public Network Network {
            get;
        }

        private volatile Dictionary<Connection, double?> ConnectionInfluence;

        private Connection[] AllConnections;

        public Backpropagate(Network network, double learningRate = 0.5) {
            Network = network;
            LearningRate = learningRate;

            //Loop trough all network connections and add them to ConnectionInfluence list
            ConnectionInfluence = new Dictionary<Connection, double?>(/*new Connection.ConnectionComparer()*/);
            for(int layer = 1; layer < Network.Nodes.Length; layer++) { //Skips input layer
                for(int index = 0; index < Network.Nodes[layer].Length; index++) {
                    //Loop trough current nodes incomming connections
                    foreach(Connection con in Network.Nodes[layer][index].GetIncommingConnections()) {
                        ConnectionInfluence.Add(con, null);
                    }
                }
            }

            AllConnections = ConnectionInfluence.Keys.ToArray();
        }

        public void Train(InputExpectedResult[] expected) {
            foreach(var exp in expected) {
                AdjustWeights(exp);
            }
        }

        private void AdjustWeights(InputExpectedResult irp) {
            ResetCurTimer();
            double[] actual = Network.GetInputResult(irp.Input);
            double[] target = irp.Output;

            LogProcess("Load actual / target");

            //Reset influence values option_0
            foreach(var key in AllConnections) {
                ConnectionInfluence[key] = null;
            }

            LogProcess("Reset influence values");

            //Set output influence value
            for(int nodeIndex = 0; nodeIndex < Network.Nodes[Network.Nodes.Length - 1].Length; nodeIndex++) {
                double curTarget = target[nodeIndex];
                double curActual = actual[nodeIndex];
                Node curNode = Network.Nodes[Network.Nodes.Length - 1][nodeIndex];

                foreach(Connection toOutputCon in curNode.GetIncommingConnections()) {
                    double preCalc = CalcOutputInfuence(toOutputCon, curTarget, curActual);
                    ConnectionInfluence[toOutputCon] = preCalc;
                }
            }

            LogProcess("Set output influence values");

            //Fill ConnectionInfluence values
            foreach(Connection connection in AllConnections) {
                GetConnectionInfluence(connection);
            }

            LogProcess("Fill ConnectionInfluence values");

            //Update weights
            foreach(KeyValuePair<Connection, double?> conInfPair in ConnectionInfluence) {
                double outputInfluence = conInfPair.Value.Value * conInfPair.Key.FromNode.Output;
                double deltaWeight = -LearningRate * outputInfluence;
                conInfPair.Key.Weight += deltaWeight;
            }

            LogProcess("Update weights");
        }

        private double CalcOutputInfuence(Connection connection, double expectedOutput, double actualOutput) {
            double dif = (-(expectedOutput - actualOutput));
            double outcome = dif * Network.TransferFunction.Derivative(actualOutput);
            return outcome;
        }

        /// <summary>
        /// Calculates connection influence value IF it is not yet present in ConnectionInfluence dictionary
        /// Stores result in the dictionary and then returns it
        /// </summary>
        private double GetConnectionInfluence(Connection connection) {
            if(!ConnectionInfluence[connection].HasValue) {
                double sumInfluenceOutput = 0;
                foreach(var outgoing in connection.ToNode.GetOutgoingConnections()) {
                    double connectionInfluence = GetConnectionInfluence(outgoing);
                    double curInfluence = connectionInfluence * outgoing.Weight;
                    sumInfluenceOutput += curInfluence;
                }

                double fromOutput = connection.FromNode.Output;
                double outDeriv = Network.TransferFunction.Derivative(connection.ToNode.Output);

                double influence = sumInfluenceOutput * outDeriv;
                ConnectionInfluence[connection] = influence;
                return influence;
            }

            return ConnectionInfluence[connection].Value;
        }
    }
}