using NeuralNet;
using NeuralNet.BackpropagationTraining;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NeuralNet.MathHelper;

namespace IrisSpecies {

    internal class Program {
        private const int TrainSetSize = 130;
        private static Random random = new Random();

        private static void Main(string[] args) {
            var data = DataReader.ReadFromFile("data/iris.data").OrderBy(i => random.Next());
            foreach(var entry in data) {
                //TODO: it is suboptimal to calculate min and max every time
                entry.PetalLength = Normalize(entry.PetalLength, data.Min(i => i.PetalLength), data.Max(i => i.PetalLength));
                entry.PetalWidth = Normalize(entry.PetalWidth, data.Min(i => i.PetalWidth), data.Max(i => i.PetalWidth));
                entry.SepalLength = Normalize(entry.SepalLength, data.Min(i => i.SepalLength), data.Max(i => i.SepalLength));
                entry.SepalWidth = Normalize(entry.SepalWidth, data.Min(i => i.SepalWidth), data.Max(i => i.SepalWidth));
            }

            var network = new Network(new NeuralNet.TransferFunctions.SigmoidFunction(), true);
            network.FillNetwork(4, 3, 100);

            var train = data.Take(TrainSetSize).ToArray();
            var inputAndExpectedResuls = train.Select(entry => new InputExpectedResult(entry.AsInput, entry.AsOutput));
            var validation = data.Skip(TrainSetSize).ToArray();

            var bp = new Backpropagate(network, inputAndExpectedResuls.ToArray(), 0.5f);

            int trains = 0;
            float percentCorrect = 0;
            while(percentCorrect <= 90) {
                trains++;
                bp.Train();

                int success = 0;
                foreach(IrisEntry validate in validation) {
                    var curOutp = network.GetInputResult(validate.AsInput);
                    IrisSpecies result = IrisEntry.SpeciesFromOutput(curOutp);

                    if(result == validate.Species) {
                        success++;
                    }
                }
                percentCorrect = 100f * success / validation.Length;
                Console.WriteLine($"#{trains} {percentCorrect}% correct");
            }

            Console.WriteLine("Done training");
            int doneSucces = 0;
            foreach(IrisEntry entry in data) {
                var curOutp = network.GetInputResult(entry.AsInput);
                IrisSpecies result = IrisEntry.SpeciesFromOutput(curOutp);

                if(result == entry.Species) {
                    doneSucces++;
                }
            }

            var percentDoneCorrect = 100 * doneSucces / data.Count();
            Console.WriteLine($" {doneSucces}/{data.Count()} {percentDoneCorrect}% correct");

            Console.ReadKey();
        }
    }
}