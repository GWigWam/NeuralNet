using NeuralNet.Connections;
using NeuralNet.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.BackpropagationTraining {

    public class Backpropagate {

        public float LearningRate {
            get; set;
        }

        public Network Network {
            get;
        }

        public InputExpectedResult[] Expected {
            get;
        }

        private Dictionary<Connection, float?> ConnectionInfluence;

        public Backpropagate(Network network, InputExpectedResult[] expected, float learningRate = 0.5f) {
            Network = network;
            Expected = expected;
            LearningRate = learningRate;
        }

        public void Train() {
            foreach(var expected in Expected) {
                AdjustWeights(expected);
            }
        }

        private void AdjustWeights(InputExpectedResult irp) {
            float[] actual = Network.GetInputResult(irp.Input);
            float[] target = irp.Output;

            var sse = SumSquaredError(target, actual);

            ConnectionInfluence = new Dictionary<Connection, float?>();

            //Loop trough all network nodes
            for(int layer = 1; layer < Network.Nodes.Length; layer++) { //Skips input layer
                for(int index = 0; index < Network.Nodes[layer].Length; index++) {
                    //Loop trough current nodes incomming connections
                    foreach(Connection con in Network.Nodes[layer][index].GetIncommingConnections()) {
                        float? precalc = null;
                        if(layer == Network.Nodes.Length - 1) { //Is output layer
                            precalc = CalcOutputInfuence(con, target[index], actual[index]);
                        }

                        ConnectionInfluence.Add(con, precalc);
                    }
                }
            }

            //Fill ConnectionInfluence values
            foreach(var con in ConnectionInfluence.Keys.ToArray()) {
                GetConnectionInfluence(con);
            }

            //Update weights
            foreach(KeyValuePair<Connection, float?> conInfPair in ConnectionInfluence) {
                float deltaWeight = -LearningRate * conInfPair.Value.Value * conInfPair.Key.FromNode.Output;
                conInfPair.Key.Weight += deltaWeight;
            }

            var newRes = Network.GetInputResult(irp.Input);
            var newSse = SumSquaredError(target, newRes);
        }

        private float CalcOutputInfuence(Connection connection, float expectedOutput, float actualOutput) {
            float outcome = Network.TransferFunction.Derivative(actualOutput) * (actualOutput - expectedOutput);
            return outcome;
        }

        /// <summary>
        /// Calculates connection influence value IF it is not yet present in ConnectionInfluence dictionary
        /// Stores result in the dictionary and then returns it
        /// </summary>
        private float GetConnectionInfluence(Connection connection) {
            if(!ConnectionInfluence[connection].HasValue) {
                float sumInfluenceOutput = 0;
                foreach(var outgoing in connection.ToNode.GetOutgoingConnections()) {
                    float curInfluence = GetConnectionInfluence(outgoing) * outgoing.Weight;
                    sumInfluenceOutput += curInfluence;
                }

                float fromOutput = connection.FromNode.Output;
                float influence = Network.TransferFunction.Derivative(fromOutput) * sumInfluenceOutput;

                ConnectionInfluence[connection] = influence;
            }

            return ConnectionInfluence[connection].Value;
        }

        public static float SquaredError(float target, float actual) {
            var val = Math.Pow(target - actual, 2);
            return (float)val;
        }

        public static float SumSquaredError(float[] target, float[] actual) {
            float sum = 0;
            for(int i = 0; i < target.Length; i++) {
                float curErr = SquaredError(target[i], actual[i]);
                sum += curErr;
            }

            return sum;
        }
    }
}