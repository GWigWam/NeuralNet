using NeuralNet;
using NeuralNet.BackpropagationTraining;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrisSpecies {

    internal class Program {
        private const int TrainSetSize = 130;

        private static void Main(string[] args) {
            IrisEntry[] data = DataReader.ReadFromFile("data/iris.data");

            var train = data.Take(TrainSetSize);
            var validation = data.Skip(TrainSetSize);

            var network = new Network(new NeuralNet.TransferFunctions.SigmoidFunction(), true);
            network.FillNetwork(4, 3, 100);
            network.RandomizeWeights();

            foreach(var entry in train) {
                var input = new float[] { entry.SepalLength, entry.SepalWidth, entry.PetalLength, entry.PetalWidth };
                var output = (entry.Species == IrisSpecies.Setosa) ?
                    new float[] { 1, 0, 0 } : (entry.Species == IrisSpecies.Versicolor) ? new float[] { 0, 1, 0 } : new float[] { 0, 0, 1 };

                var ipr = new InputResultPair(input, output);

                var res = network.GetInputResult(entry.SepalLength, entry.SepalWidth, entry.PetalLength, entry.PetalWidth);
            }
        }
    }
}