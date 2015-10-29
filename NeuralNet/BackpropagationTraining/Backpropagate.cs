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

        public Backpropagate(Network network, double learningRate = 0.5) {
            Network = network;
            LearningRate = learningRate;

            ConnectionInfluence = new Dictionary<Connection, double?>();
        }

        public void Train(InputExpectedResult[] expected) {
            foreach(var exp in expected) {
                AdjustWeights(exp);
            }
        }

        private void AdjustWeights(InputExpectedResult irp) {
            double[] actual = Network.GetInputResult(irp.Input);
            double[] target = irp.Output;

            ConnectionInfluence.Clear();

            //Loop trough all network nodes
            for(int layer = 1; layer < Network.Nodes.Length; layer++) { //Skips input layer
                for(int index = 0; index < Network.Nodes[layer].Length; index++) {
                    //Loop trough current nodes incomming connections
                    foreach(Connection con in Network.Nodes[layer][index].GetIncommingConnections()) {
                        double? precalc = null;
                        if(layer == Network.Nodes.Length - 1) { //Is output layer
                            precalc = CalcOutputInfuence(con, target[index], actual[index]);
                        }

                        ConnectionInfluence.Add(con, precalc);
                    }
                }
            }
            Log("Setup outputs");

            //Fill ConnectionInfluence values
            Parallel.ForEach(ConnectionInfluence.Keys.ToArray(),
#if DEBUG
                new ParallelOptions() { MaxDegreeOfParallelism = 1 }, // When debuggin don't use parallelism
#endif
                (con) => GetConnectionInfluence(con));

            Log("Setup other");

            //Update weights
            foreach(KeyValuePair<Connection, double?> conInfPair in ConnectionInfluence) {
                double outputInfluence = conInfPair.Value.Value * conInfPair.Key.FromNode.Output;
                double deltaWeight = -LearningRate * outputInfluence;
                conInfPair.Key.Weight += deltaWeight;
            }

            Log("Fix weights");
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
            }

            return ConnectionInfluence[connection].Value;
        }
    }
}