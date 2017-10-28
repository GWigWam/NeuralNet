using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralNet.Base;

namespace NeuralNet.BackpropagationTraining {
    public class Backpropagate2 {
        public double LearningRate { get; set; }

        public INetwork Network { get; }

        public Backpropagate2(INetwork network, double learningRate = 0.5) {
            Network = network;
            LearningRate = learningRate;
        }

        public void Train(InputExpectedResult[] inputExpectedResultPairs) {
            foreach (var trainingEntry in inputExpectedResultPairs) {
                var networkState = Network.GetValuesForInput(trainingEntry.Input.Select(dbl => (float)dbl).ToArray());
                var actualOutput = networkState[Network.LayerCount - 1];
                                
                // Calculate weight errors
                var weightErrors = new ConErr[Network.LayerCount - 1][][]; // [Layer] [FromNode] [ToNode]

                for (int layNr = weightErrors.Length - 1; layNr >= 0; layNr--) { //Starts at the output, this is BACKprop
                    int curLayHeight = Network.GetLayerHeight(layNr);
                    int nextLayHeight = Network.GetLayerHeight(layNr + 1);

                    weightErrors[layNr] = new ConErr[curLayHeight][];

                    for (int fromNodeNr = 0; fromNodeNr < curLayHeight; fromNodeNr++) {
                        weightErrors[layNr][fromNodeNr] = new ConErr[nextLayHeight];
                        for (int toNodeNr = 0; toNodeNr < nextLayHeight; toNodeNr++) {

                            float outputError;
                            if (layNr == weightErrors.Length - 1) { // If output layer
                                outputError = (float)-(trainingEntry.ExpectedOutput[toNodeNr] - actualOutput[toNodeNr]); //Error based on expected vs actual
                            } else {
                                var influencedWeights = Network.Weights[layNr + 1][toNodeNr];
                                var influencedErrors = weightErrors[layNr + 1][toNodeNr].Select(e => e.OutputError * e.OutputDerivative);
                                var outputErrors = influencedWeights.Zip(influencedErrors, (w, e) => w * e);
                                outputError = outputErrors.Sum();
                            }

                            var toNodeOutp = networkState[layNr + 1][toNodeNr];
                            var toNodeDer = Network.TransferFunction.Derivative(toNodeOutp);
                            var conInputValue = networkState[layNr][fromNodeNr];

                            weightErrors[layNr][fromNodeNr][toNodeNr] = new ConErr(outputError, (float)toNodeDer, conInputValue);
                        }
                    }
                }

                //Update weights based on errors
                for (int l = 0; l < weightErrors.Length; l++) {
                    for (int f = 0; f < weightErrors[l].Length; f++) {
                        for (int t = 0; t < weightErrors[l][f].Length; t++) {
                            var curErr = weightErrors[l][f][t];
                            var error = curErr.OutputError * curErr.OutputDerivative * curErr.ConnectionInputValue;
                            var deltaWeight = error * LearningRate;
                            
                            Network.Weights[l][f][t] -= (float)deltaWeight;
                        }
                    }
                }
            }
        }

        private struct ConErr {
            public readonly float OutputError;
            public readonly float OutputDerivative;
            public readonly float ConnectionInputValue;

            public ConErr(float outpErr, float outpDer, float conInVal) {
                OutputError = outpErr;
                OutputDerivative = outpDer;
                ConnectionInputValue = conInVal;
            }
        }
    }
}
