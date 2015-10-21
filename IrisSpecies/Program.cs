using NeuralNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrisSpecies {

    internal class Program {
        private const int TrainSetSize = 130;

        private static void Main(string[] args) {
            IrisEntry[] data = DataReader.ReadFromFile("iris.data");

            var train = data.Take(TrainSetSize);
            var validation = data.Skip(TrainSetSize);

            var network = new Network(new NeuralNet.TransferFunctions.SigmoidFunction());
            network.FillNetwork(4, 3, 100);
        }
    }
}