using NeuralNet.Base;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet {
    public class Network2 : INetwork {
        public TransferFunction TransferFunction { get; }

        public int LayerCount => Weights.Length + 1;
        public int HiddenLayerCount => Weights.Length - 1;

        public float[][][] Weights { get; private set; }

        public float[][] BiasWeights { get; private set; }

        public int[] LayerHeights { get; private set; }

        public Network2(TransferFunction tf) {
            TransferFunction = tf;
        }

        public void FillNetwork(int nrInputs, int nrOutputs, params int[] hiddenLayerHeights) {
            var conLayCount = hiddenLayerHeights.Length + 1;
            Weights = new float[conLayCount][][];
            BiasWeights = new float[conLayCount][];
            for (int layNr = 0; layNr < conLayCount; layNr++) {
                int fromLayHeight = layNr == 0 ? nrInputs : hiddenLayerHeights[layNr - 1];
                int toLayHeight = layNr == hiddenLayerHeights.Length ? nrOutputs : hiddenLayerHeights[layNr];

                Weights[layNr] = new float[fromLayHeight][];
                for (int fromNr = 0; fromNr < fromLayHeight; fromNr++) {
                    Weights[layNr][fromNr] = new float[toLayHeight];
                    for (int toNr = 0; toNr < toLayHeight; toNr++) {
                        Weights[layNr][fromNr][toNr] = (float)MathHelper.GuassianRandom(Math.Sqrt(1.0 / 3.0), 0);
                    }
                }

                BiasWeights[layNr] = new float[toLayHeight];
                for (int toNr = 0; toNr < toLayHeight; toNr++) {
                    BiasWeights[layNr][toNr] = (float)MathHelper.GuassianRandom(Math.Sqrt(1.0 / 3.0), 0);
                }
            }

            LayerHeights = new[] { nrInputs }.Concat(hiddenLayerHeights).Concat(new[] { nrOutputs }).ToArray();
        }

        public float[][] GetValuesForInput(params float[] input) {
            float[][] layerResults = new float[LayerCount][];
            layerResults[0] = input;
            for (int layNr = 0; layNr < Weights.Length; layNr++) {                
                var curLayerWeights = Weights[layNr];
                var curLayerRes = new float[curLayerWeights[0].Length];
                for (int toNr = 0; toNr < curLayerWeights[0].Length; toNr++) {
                    var sum = 0f;
                    for (int fromNr = 0; fromNr < curLayerWeights.Length; fromNr++) {
                        var nodeVal = layerResults[layNr][fromNr];
                        var conWeight = curLayerWeights[fromNr][toNr];
                        var weighted = nodeVal * conWeight;
                        sum += weighted;
                    }

                    var biasWeight = BiasWeights[layNr][toNr];
                    sum += 1f * biasWeight;

                    var res = (float)TransferFunction.Calculate(sum);
                    curLayerRes[toNr] = res;
                }
                
                layerResults[layNr + 1] = curLayerRes;
            }
            return layerResults;
        }

        public float[] GetOutputForInput(params float[] input) => GetValuesForInput(input).Last();

        public int GetLayerHeight(int layerNr) => LayerHeights[layerNr];
    }
}
