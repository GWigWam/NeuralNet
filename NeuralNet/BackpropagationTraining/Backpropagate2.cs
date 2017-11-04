using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralNet.Base;

namespace NeuralNet.BackpropagationTraining {
    public class Backpropagate2 {
        public float LearningRate { get; set; }

        public Network2 Network { get; }

        public Backpropagate2(Network2 network, float learningRate = 0.5f) {
            Network = network;
            LearningRate = learningRate;
        }

        public void Train(InputExpectedResult[] inputExpectedResultPairs) {
            foreach (var trainingEntry in inputExpectedResultPairs) {
                var networkState = Network.GetValuesForInput(trainingEntry.Input.Select(dbl => (float)dbl).ToArray());
                var actualOutput = networkState[Network.LayerCount - 1];

                // These 'ConErr' objects are used to determine the eventual weight adjustment for this pass.
                var weightErrors = new ConErr[Network.LayerCount - 1][][]; // [Layer] [FromNode] [ToNode]
                var biasErrors = new ConErr[Network.LayerCount - 1][]; // [Layer - 1] [Node]

                // Loop through layers backwards
                // Starts at the output, this is BACKprop
                for (int layNr = weightErrors.Length - 1; layNr >= 0; layNr--) {
                    int curLayHeight = Network.GetLayerHeight(layNr);
                    int nextLayHeight = Network.GetLayerHeight(layNr + 1);

                    weightErrors[layNr] = new ConErr[curLayHeight][];
                    biasErrors[layNr] = new ConErr[nextLayHeight];

                    // Calc weight error
                    for (int fromNodeNr = 0; fromNodeNr < curLayHeight; fromNodeNr++) {
                        weightErrors[layNr][fromNodeNr] = new ConErr[nextLayHeight];
                        for (int toNodeNr = 0; toNodeNr < nextLayHeight; toNodeNr++) {

                            float outputError;
                            if (layNr == weightErrors.Length - 1) {
                                // Error of connection going to the output layers is based on expected vs actual output
                                outputError = -(trainingEntry.ExpectedOutput[toNodeNr] - actualOutput[toNodeNr]);
                            } else {
                                // Error of connection going to hidden layers
                                var influencedWeights = Network.Weights[layNr + 1][toNodeNr];
                                var influencedErrors = weightErrors[layNr + 1][toNodeNr].Select(e => e.OutputError * e.OutputDerivative);
                                var outputErrors = influencedWeights.Zip(influencedErrors, (w, e) => w * e);
                                outputError = outputErrors.Sum();
                            }

                            var toNodeOutp = networkState[layNr + 1][toNodeNr];
                            var toNodeDer = Network.TransferFunction.Derivative(toNodeOutp);

                            var conInputValue = networkState[layNr][fromNodeNr];

                            weightErrors[layNr][fromNodeNr][toNodeNr] = new ConErr(outputError, toNodeDer, conInputValue);

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
                            var nError = weightErrors[l][f][t].Delta;
                            var nDeltaWeight = nError * LearningRate;
                            
                            Network.Weights[l][f][t] -= nDeltaWeight;

                            // Adjust bias
                            if (f == 0) {
                                var bError = biasErrors[l][t].Delta;
                                var bDeltaWeight = bError * LearningRate;

                                Network.BiasWeights[l][t] -= bDeltaWeight;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Delta (change) of a weight is calculated based on: '<see cref="OutputError"/>', '<see cref="OutputDerivative"/>', '<see cref="ConnectionInputValue"/>'
        /// </summary>
        private struct ConErr {
            /// <summary>
            /// Difference between actual and expected optimal output.
            /// </summary>
            public readonly float OutputError;

            /// <summary>
            /// Result of passing the ouput of the connections destination node trought the transfer functions derivative <para />
            /// E.g. TransferFunc.CalcDerivative(Connection.DestinationNode.OutputValue)
            /// </summary>
            public readonly float OutputDerivative;

            /// <summary>
            /// Input into this connection, this is the output of the FromNode
            /// </summary>
            public readonly float ConnectionInputValue;

            public ConErr(float outpErr, float outpDer, float conInVal) {
                OutputError = outpErr;
                OutputDerivative = outpDer;
                ConnectionInputValue = conInVal;
            }

            /// <summary>
            /// Weight change (delta) that will improve node output
            /// </summary>
            public float Delta => OutputError * OutputDerivative * ConnectionInputValue;
        }
    }
}
