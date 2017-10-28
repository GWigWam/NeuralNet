using NeuralNet;
using NeuralNet.BackpropagationTraining;
using NeuralNet.Base;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NeuralNet.MathHelper;

namespace IrisSpecies {

    internal class Program {
        private const int TrainSetSize = 130;
        private static Random random = new Random(4);

        private static TransferFunction Transfer = new HyperbolicTangentFunction();

        //private static INetwork Network = new Network2(Transfer);
        private static Network Network = new Network(Transfer, true);

        private static void Main(string[] args) {
            var data = DataReader.ReadFromFile("data/iris.data")/*.OrderBy(i => random.Next())*/;
            foreach(var entry in data) {
                //TODO: it is suboptimal to calculate min and max every time
                entry.PetalLength = (float)Normalize(entry.PetalLength, data.Min(i => i.PetalLength), data.Max(i => i.PetalLength));
                entry.PetalWidth = (float)Normalize(entry.PetalWidth, data.Min(i => i.PetalWidth), data.Max(i => i.PetalWidth));
                entry.SepalLength = (float)Normalize(entry.SepalLength, data.Min(i => i.SepalLength), data.Max(i => i.SepalLength));
                entry.SepalWidth = (float)Normalize(entry.SepalWidth, data.Min(i => i.SepalWidth), data.Max(i => i.SepalWidth));
            }
            
            Network.FillNetwork(4, 3, 6);

            var train = data.Take(TrainSetSize).ToArray();
            var inputAndExpectedResuls = train.Select(entry => new InputExpectedResult(entry.AsInput, entry.AsOutput));
            var validation = data.Skip(TrainSetSize).ToArray();
            
            var trainData = inputAndExpectedResuls.ToArray();

            //var Bp = new Backpropagate2(Network, 0.5);
            var Bp = new Backpropagate(Network, 0.5);

            int trains = 0;
            double score = 0;
            var start = Environment.TickCount;
            while(score < 90) {
                trains++;
                Bp.Train(trainData.OrderBy(i => random.Next()).ToArray());

                var stats = NetworkValidation.Validate(Network, inputAndExpectedResuls, IrisEntry.IsOutputSuccess);
                score = stats.SuccessPercentage;
                Console.WriteLine($"{trains,-4}" + stats.ToString());
            }
            Console.WriteLine($"\nTime elapsed: {Environment.TickCount - start}Ms");
            Console.WriteLine("Done training");

            var trainStats = NetworkValidation.Validate(Network, train.Select(entry => new InputExpectedResult(entry.AsInput, entry.AsOutput)), IrisEntry.IsOutputSuccess);
            var validateStats = NetworkValidation.Validate(Network, validation.Select(entry => new InputExpectedResult(entry.AsInput, entry.AsOutput)), IrisEntry.IsOutputSuccess);

            Console.WriteLine($"{trainStats.ToString()} TRAIN");
            Console.WriteLine($"{validateStats.ToString()} VALIDATE");
            Console.WriteLine($"{(trainStats + validateStats).ToString()} TOTAL");

            Console.ReadKey();
        }
    }
}