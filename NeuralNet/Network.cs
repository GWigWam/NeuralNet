using NeuralNet.Connections;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet {

    public class Network {

        private TransferFunction TransferFunction {
            get;
        }

        public Network(TransferFunction tf) {
            TransferFunction = tf;
        }

        // Perceptrons [Column] [Index] where column 0 is the input layer
        public Perceptron[][] Perceptrons {
            get; set;
        }

        public void FillNetwork(int nrInputs, int nrOutputs, params int[] hiddenLayerHeights) {
            //Creation of outputs is similar to creating hidden layers
            var nonInputLayers = hiddenLayerHeights.ToList();
            nonInputLayers.Add(nrOutputs);

            //Init array
            Perceptrons = new Perceptron[nonInputLayers.Count + 1][];

            //Inputs
            Perceptrons[0] = CreateInputs(nrInputs).ToArray();

            //Hidden & out
            for(int layerIndex = 1; layerIndex <= nonInputLayers.Count; layerIndex++) {
                int height = nonInputLayers[layerIndex - 1];

                var curLayer = new List<Perceptron>(height);
                float connectionWeight = 1f / Perceptrons[layerIndex - 1].Length; //1.0 div. by Prev layer length
                for(int percNr = 0; percNr < height; percNr++) {
                    IEnumerable<WeightedConnection> connections = Perceptrons[layerIndex - 1].Select(p => new WeightedConnection(connectionWeight, () => p.Output));

                    string percName = layerIndex == Perceptrons.Length - 1 ? $"Output {percNr}" : $"Hidden #{layerIndex - 1}.{percNr}";
                    var curPerceptron = new Perceptron(TransferFunction, connections, percName);
                    curLayer.Add(curPerceptron);
                }
                Perceptrons[layerIndex] = curLayer.ToArray();
            }
        }

        public float[] GetResult() {
            ResetPerceptronCache();
            return Perceptrons[Perceptrons.Length - 1].Select(p => p.Output).ToArray();
        }

        public void ResetPerceptronCache() {
            foreach(var perc in Perceptrons.SelectMany(p => p)) {
                perc.ResetCache();
            }
        }

        private IEnumerable<Perceptron> CreateInputs(int count) {
            for(int i = 0; i < count; i++) {
                yield return new Perceptron(TransferFunction, new SetValueConnection(0), $"Input {i}");
            }
        }
    }
}