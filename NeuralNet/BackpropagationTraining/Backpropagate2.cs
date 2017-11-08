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

        public void Train(InputExpectedResult[] inputExpectedResultPairs, int miniBatchSize) {
            int i = 0;
            while (i < inputExpectedResultPairs.Length) {
                Train(inputExpectedResultPairs.Skip(i).Take(miniBatchSize).ToArray());
                i += miniBatchSize;
            }
        }

        public void Train(InputExpectedResult[] miniBatch) {
            var trainSetSize = miniBatch.Length;
            var weightLayerCount = Network.LayerCount - 1;

            // Init arrays
            // These 'ConErr' objects are used to determine the eventual weight adjustment for this pass.
            var weightErrors = new ConErr[weightLayerCount][][][]; // [Layer] [FromNode] [ToNode] [TrainingItem]
            var biasErrors = new ConErr[weightLayerCount][][]; // [Layer - 1] [Node] [TrainingItem]
            for (int layNr = 0; layNr < weightLayerCount; layNr++) {
                int curLayHeight = Network.GetLayerHeight(layNr);
                int nextLayHeight = Network.GetLayerHeight(layNr + 1);
                weightErrors[layNr] = new ConErr[curLayHeight][][];
                biasErrors[layNr] = new ConErr[nextLayHeight][];

                // Init inner 'weightError' array
                for (int fromNodeNr = 0; fromNodeNr < curLayHeight; fromNodeNr++) {
                    weightErrors[layNr][fromNodeNr] = new ConErr[nextLayHeight][];
                    for (int toNodeNr = 0; toNodeNr < nextLayHeight; toNodeNr++) {
                        weightErrors[layNr][fromNodeNr][toNodeNr] = new ConErr[trainSetSize];
                    }
                }

                // Init inner 'biasError' array
                for (int toNodeNr = 0; toNodeNr < nextLayHeight; toNodeNr++) {
                    biasErrors[layNr][toNodeNr] = new ConErr[trainSetSize];
                }
            }

            // Calculate weight errors foreach training entry in the mini-batch
            //for (int trainNr = 0; trainNr < trainSetSize; trainNr++) {
            Parallel.For(0, trainSetSize, trainNr => {
                var trainingEntry = miniBatch[trainNr];
                var networkState = Network.GetValuesForInput(trainingEntry.Input);

                // Loop through layers backwards
                // Starts at the output, this is BACKprop
                for (int layNr = weightErrors.Length; --layNr >= 0;) {
                    for (int fromNodeNr = 0; fromNodeNr < weightErrors[layNr].Length; fromNodeNr++) {
                        for (int toNodeNr = 0; toNodeNr < weightErrors[layNr][fromNodeNr].Length; toNodeNr++) {

                            float outputError;
                            if (layNr == weightErrors.Length - 1) {
                                // Error of connection going to the output layers is based on expected vs actual output
                                outputError = -(trainingEntry.ExpectedOutput[toNodeNr] - networkState[layNr + 1][toNodeNr]);
                            } else {
                                // Error of connection going to hidden layers
                                var influencedWeights = Network.Weights[layNr + 1][toNodeNr];
                                var influencedErrors = weightErrors[layNr + 1][toNodeNr].Select(e => e[trainNr].OutputError * e[trainNr].OutputDerivative);
                                var outputErrors = influencedWeights.Zip(influencedErrors, Multiply);
                                outputError = outputErrors.Sum();
                            }

                            var toNodeOutp = networkState[layNr + 1][toNodeNr];
                            var toNodeDer = Network.TransferFunction.Derivative(toNodeOutp);

                            var conInputValue = networkState[layNr][fromNodeNr];

                            weightErrors[layNr][fromNodeNr][toNodeNr][trainNr] = new ConErr(outputError, toNodeDer, conInputValue);

                            // Calc bias error
                            if (fromNodeNr == 0) {
                                biasErrors[layNr][toNodeNr][trainNr] = new ConErr(outputError, toNodeDer, 1 /*Bias input is alway 1*/);
                            }
                        }
                    }
                }
            });

            //Update weights based on errors
            for (int layerNr = 0; layerNr < weightLayerCount; layerNr++) {
                for (int fromNodeNr = 0; fromNodeNr < weightErrors[layerNr].Length; fromNodeNr++) {
                    for (int toNodeNr = 0; toNodeNr < weightErrors[layerNr][fromNodeNr].Length; toNodeNr++) {
                        var nErrorPerSample = weightErrors[layerNr][fromNodeNr][toNodeNr].Select(err => err.Delta);
                        var nAvgError = nErrorPerSample.Average();
                        var nDeltaWeight = nAvgError * LearningRate;

                        Network.Weights[layerNr][fromNodeNr][toNodeNr] -= nDeltaWeight;

                        // Adjust bias
                        if (fromNodeNr == 0) {
                            var bErrorPerSample = biasErrors[layerNr][toNodeNr].Select(err => err.Delta);
                            var bAvgError = bErrorPerSample.Average();
                            var bDeltaWeight = bAvgError * LearningRate;

                            Network.BiasWeights[layerNr][toNodeNr] -= bDeltaWeight;
                        }
                    }
                }
            }
        }

        private static float Multiply(float f1, float f2) => f1 * f2;

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
