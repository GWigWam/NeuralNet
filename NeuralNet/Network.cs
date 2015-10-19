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

                    var curPerceptron = new Perceptron(TransferFunction, connections);
                    curLayer.Add(curPerceptron);
                }
                Perceptrons[layerIndex] = curLayer.ToArray();
            }
        }

        private IEnumerable<Perceptron> CreateInputs(int count) {
            for(int i = 0; i < count; i++) {
                yield return new Perceptron(TransferFunction, new SetValueConnection(0));
            }
        }
    }
}