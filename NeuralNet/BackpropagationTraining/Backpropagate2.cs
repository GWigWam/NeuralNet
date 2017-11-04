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
                var biasErrors = new ConErr[Network.LayerCount - 1][]; // [Layer - 1] [Node]

                for (int layNr = weightErrors.Length - 1; layNr >= 0; layNr--) { //Starts at the output, this is BACKprop
                    int curLayHeight = Network.GetLayerHeight(layNr);
                    int nextLayHeight = Network.GetLayerHeight(layNr + 1);

                    weightErrors[layNr] = new ConErr[curLayHeight][];
                    biasErrors[layNr] = new ConErr[nextLayHeight];

                    // Calc weight error
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

                            // Calc bias error
                            if (fromNodeNr == 0) {
                                biasErrors[layNr][toNodeNr] = new ConErr(outputError, toNodeDer, 1 /*Bias input is alway 1*/);
                            }
                        }
                    }
                }

                //Update weights based on errors
                for (int l = 0; l < weightErrors.Length; l++) {
                    for (int f = 0; f < weightErrors[l].Length; f++) {
                        for (int t = 0; t < weightErrors[l][f].Length; t++) {
                            var nErrEntry = weightErrors[l][f][t];
                            var nError = nErrEntry.OutputError * nErrEntry.OutputDerivative * nErrEntry.ConnectionInputValue;
                            var nDeltaWeight = nError * LearningRate;
                            
                            Network.Weights[l][f][t] -= (float)nDeltaWeight;

                            // Adjust bias
                            if (f == 0) {
                                var bErrEntry = biasErrors[l][t];
                                var bError = bErrEntry.OutputError * bErrEntry.OutputDerivative * bErrEntry.ConnectionInputValue;
                                var bDeltaWeight = bError * LearningRate;

                                Network.BiasWeights[l][t] -= (float)bDeltaWeight;
                            }
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
