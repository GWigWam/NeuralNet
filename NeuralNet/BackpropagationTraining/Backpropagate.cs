using NeuralNet.Connections;
using NeuralNet.Nodes;
using System;
using System.Collections.Concurrent;
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

        public int MicroBatchSize {
            get;
        }

        public Network Network {
            get;
        }

        private volatile ConcurrentDictionary<Connection, float?[]> ConnectionInfluence;

        private readonly Connection[] AllConnections;

        public Backpropagate(Network network, double learningRate = 0.5, int microBatchSize = 1) {
            Network = network;
            LearningRate = learningRate;
            MicroBatchSize = microBatchSize;

            //Loop trough all network connections and add them to ConnectionInfluence list
            ConnectionInfluence = new ConcurrentDictionary<Connection, float?[]>();
            var fillAllCollections = new List<Connection>();
            for(int layer = 1; layer < Network.Nodes.Length; layer++) { //Skips input layer
                for(int index = 0; index < Network.Nodes[layer].Length; index++) {
                    //Loop trough current nodes incomming connections
                    foreach(Connection con in Network.Nodes[layer][index].GetIncommingConnections()) {
                        ConnectionInfluence.TryAdd(con, new float?[MicroBatchSize]);
                        fillAllCollections.Add(con);
                    }
                }
            }

            AllConnections = fillAllCollections.ToArray();
        }

        public void Train(InputExpectedResult[] expected) {
            for(int batchNr = 0; batchNr * MicroBatchSize < expected.Length; batchNr++) {
                //Reset influence values
                foreach(var key in AllConnections) {
                    ConnectionInfluence[key] = new float?[MicroBatchSize];
                }
                LogProcess("Reset influence values");

                Parallel.For(0, MicroBatchSize, (batchIndex) => {
                    if((batchNr * MicroBatchSize) + batchIndex < expected.Length) {
                        AdjustWeights(expected[(batchNr * MicroBatchSize) + batchIndex], batchIndex);
                    }
                });

                //Update weights
                foreach(var con in AllConnections) {
                    var conInfPair = ConnectionInfluence[con];

                    //TODO
                    // Following uses the node 'Output', is this ok?
                    // The output depends on current network state, this might be changed when working in parallel (microbatches, see Parallel.For)
                    float outputInfluence = conInfPair.Sum(d => (d ?? 0) * con.FromNode.Output);
                    double deltaWeight = -LearningRate * outputInfluence;
                    con.Weight += (float)deltaWeight;
                }
                LogProcess("Update weights");
            }
        }

        private void AdjustWeights(InputExpectedResult irp, int microBatchIndex) {
            ResetCurTimer();
            float[] actual = Network.GetOutputForInput(irp.Input);
            float[] target = irp.ExpectedOutput;
            LogProcess("Load actual / target");

            //Set output influence value
            for(int nodeIndex = 0; nodeIndex < Network.Nodes[Network.Nodes.Length - 1].Length; nodeIndex++) {
                float curTarget = target[nodeIndex];
                float curActual = actual[nodeIndex];
                Node curNode = Network.Nodes[Network.Nodes.Length - 1][nodeIndex];

                foreach(Connection toOutputCon in curNode.GetIncommingConnections()) {
                    var preCalc = CalcOutputInfuence(toOutputCon, curTarget, curActual);
                    ConnectionInfluence[toOutputCon][microBatchIndex] = preCalc;
                }
            }

            LogProcess("Set output influence values");

            //Fill ConnectionInfluence values
            foreach(Connection connection in AllConnections) {
                GetConnectionInfluence(connection, microBatchIndex);
            }

            LogProcess("Fill ConnectionInfluence values");
        }

        private float CalcOutputInfuence(Connection connection, float expectedOutput, float actualOutput) {
            var dif = (-(expectedOutput - actualOutput));
            var outcome = dif * Network.TransferFunction.Derivative(actualOutput);
            return outcome;
        }

        /// <summary>
        /// Calculates connection influence value IF it is not yet present in ConnectionInfluence dictionary
        /// Stores result in the dictionary and then returns it
        /// </summary>
        private float GetConnectionInfluence(Connection connection, int microBatchIndex) {
            float? cur = ConnectionInfluence[connection][microBatchIndex];
            if(cur.HasValue) {
                return cur.Value;
            } else {
                float sumInfluenceOutput = 0f;
                foreach(var outgoing in connection.ToNode.GetOutgoingConnections()) {
                    float connectionInfluence = GetConnectionInfluence(outgoing, microBatchIndex);
                    float curInfluence = connectionInfluence * outgoing.Weight;
                    sumInfluenceOutput += curInfluence;
                }

                float fromOutput = connection.FromNode.Output;
                float outDeriv = Network.TransferFunction.Derivative(connection.ToNode.Output);

                float influence = sumInfluenceOutput * outDeriv;
                ConnectionInfluence[connection][microBatchIndex] = influence;
                return influence;
            }
        }
    }
}