using NeuralNet.Connections;
using NeuralNet.Nodes;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet {

    public class Network {
        private Random random;

        private TransferFunction TransferFunction {
            get;
        }

        public Network(TransferFunction tf, bool createBiasNode) {
            TransferFunction = tf;
            random = new Random();

            if(createBiasNode) {
                Bias = new Bias();
            }
        }

        public Bias Bias {
            get;
        }

        // Perceptrons [Column] [Index] where column 0 is the input layer
        public INode[][] Nodes {
            get; set;
        }

        public void FillNetwork(int nrInputs, int nrOutputs, params int[] hiddenLayerHeights) {
            //Creation of outputs is similar to creating hidden layers
            var nonInputLayers = hiddenLayerHeights.ToList();
            nonInputLayers.Add(nrOutputs);

            //Init array
            Nodes = new INode[nonInputLayers.Count + 1][];

            //Inputs
            Nodes[0] = CreateInputs(nrInputs).ToArray();

            //Hidden & out
            for(int layerIndex = 1; layerIndex <= nonInputLayers.Count; layerIndex++) {
                int height = nonInputLayers[layerIndex - 1];

                var curLayer = new List<Perceptron>(height);
                float connectionWeight = 1f / Nodes[layerIndex - 1].Length; //1.0 div. by Prev layer length
                for(int percNr = 0; percNr < height; percNr++) {
                    //Create input connections
                    var connections = new List<WeightedConnection>();
                    connections.AddRange(Nodes[layerIndex - 1].Select(p => new WeightedConnection(connectionWeight, p)));
                    if(Bias != null) {
                        connections.Add(new WeightedConnection(0.5f, Bias));
                    }

                    //Create perceptron
                    string percName = layerIndex == Nodes.Length - 1 ? $"Output {percNr}" : $"Hidden #{layerIndex - 1}.{percNr}";
                    var curPerceptron = new Perceptron(TransferFunction, connections, percName);
                    curLayer.Add(curPerceptron);
                }
                Nodes[layerIndex] = curLayer.ToArray();
            }
        }

        public float[] GetInputResult(params float[] input) {
            if(input.Length != Nodes[0].Length) {
                throw new ArgumentOutOfRangeException(nameof(input));
            }

            for(int i = 0; i < input.Length; i++) {
                ((Input)Nodes[0][i]).Value = input[i];
            }

            return CurOutput();
        }

        public float[] CurOutput(bool ResetCache = true) {
            if(ResetCache) {
                ResetPerceptronCache();
            }
            return Nodes[Nodes.Length - 1].Select(p => p.Output).ToArray();
        }

        public void RandomizeWeights() {
            foreach(Connection connection in Nodes.SelectMany(p => p).SelectMany(p => (p as Perceptron)?.Input)) {
                if(connection is WeightedConnection) {
                    ((WeightedConnection)connection).Weight = (float)random.NextDouble();
                }
            }
        }

        public void ResetPerceptronCache() {
            foreach(INode perc in Nodes.SelectMany(p => p)) {
                if(perc is Perceptron) {
                    ((Perceptron)perc).ResetCache();
                }
            }
        }

        private IEnumerable<Input> CreateInputs(int count) {
            for(int i = 0; i < count; i++) {
                yield return new Input(0, $"Input {i}");
            }
        }
    }
}