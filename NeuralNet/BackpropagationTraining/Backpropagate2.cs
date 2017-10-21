using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.BackpropagationTraining {
    public class Backpropagate2 {
        public double LearningRate { get; set; }

        public Network Network { get; }

        public Backpropagate2(Network network, double learningRate = 0.5) {
            Network = network;
            LearningRate = learningRate;
        }

        public void Train(InputExpectedResult[] inputExpectedResultPairs) {
            foreach (var trainingEntry in inputExpectedResultPairs) {
                var networkState = Network.GetValuesForInput(trainingEntry.Input);
                var actualOutput = networkState[Network.Nodes.Length - 1];
                                
                // Calculate weight errors
                var weightErrors = new (double outpError, double outpDer, double conInputValue)[Network.Nodes.Length - 1][][]; // [Layer] [FromNode] [ToNode]
                for (int layNr = weightErrors.Length - 1; layNr >= 0; layNr--) { //Starts at the output, this is BACKprop
                    int curLayHeight = Network.Nodes[layNr].Length;
                    int nextLayHeight = Network.Nodes[layNr + 1].Length;
                    weightErrors[layNr] = new (double, double, double)[curLayHeight][];
                    for (int fromNodeNr = 0; fromNodeNr < curLayHeight; fromNodeNr++) {
                        weightErrors[layNr][fromNodeNr] = new (double, double, double)[nextLayHeight];
                        for (int toNodeNr = 0; toNodeNr < nextLayHeight; toNodeNr++) {

                            double outputError;
                            if (layNr == weightErrors.Length - 1) { // If output layer
                                outputError = -(trainingEntry.ExpectedOutput[toNodeNr] - actualOutput[toNodeNr]); //Error based on expected vs actual
                            } else {
                                var influencedWeights = Network.Nodes[layNr + 1][toNodeNr].GetOutgoingConnections().Select(c => c.Weight);
                                var influencedErrors = weightErrors[layNr + 1][toNodeNr].Select(e => e.outpError * e.outpDer);
                                var outputErrors = influencedWeights.Zip(influencedErrors, (w, e) => w * e);
                                outputError = outputErrors.Sum();
                            }

                            var toNodeOutp = networkState[layNr + 1][toNodeNr];
                            var toNodeDer = Network.TransferFunction.Derivative(toNodeOutp);
                            var conInputValue = networkState[layNr][fromNodeNr];

                            weightErrors[layNr][fromNodeNr][toNodeNr] = (outputError, toNodeDer, conInputValue);
                        }
                    }
                }

                //Update weights based on errors
                for (int l = 0; l < weightErrors.Length; l++) {
                    for (int f = 0; f < weightErrors[l].Length; f++) {
                        for (int t = 0; t < weightErrors[l][f].Length; t++) {
                            var (e, d, i) = weightErrors[l][f][t];
                            var error = e * d * i;
                            var deltaWeight = error * LearningRate;

                            var con = Network.Nodes[l][f].GetOutgoingConnections()[t];
                            con.Weight = con.Weight - deltaWeight;
                        }
                    }
                }
            }
        }
    }
}
