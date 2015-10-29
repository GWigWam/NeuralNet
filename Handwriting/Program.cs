using NeuralNet;
using NeuralNet.BackpropagationTraining;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NeuralNet.PerformanceLog;

namespace Handwriting {

    public class Program {
        private const string dirLoc = "F:/Zooi/github/NeuralNet/Handwriting/data/img";
        private const int dimentions = 32;

        private static void Main(string[] args) {
            Log("Start");
            var imgLoader = new LazyTrainImgLoader(dirLoc, true, true, true, dimentions, 100);

            Log("ImageLoader read, create & train network");
            var network = new Network(new SigmoidFunction(), true);
            network.FillNetwork(dimentions * dimentions, 10, 15);

            var backpropTraining = new Backpropagate(network, 0.5);

            bool cancelTrain = false;
            Console.CancelKeyPress += (obj, evArgs) => {
                if(!cancelTrain) {
                    cancelTrain = true;
                    evArgs.Cancel = false;
                }
            };

            InputExpectedResult[] trainData;
            while(!cancelTrain && (trainData = imgLoader.GetNextBatch()).Length > 0) {
                backpropTraining.Train(trainData);
                Log("End single train");

                var stats = NetworkValidation.Validate(network, trainData, IsImgRecogSuccess);
                Console.WriteLine($"{imgLoader.Index,-4} / {imgLoader.FileCount,-5} | " + stats.ToString());
                Log("End stat gather");
            }
            Log("Done training");

            Console.ReadKey();
        }

        private static void PrintGreyValueImg(double[] greyValues) {
            int dimentions = (int)Math.Sqrt(greyValues.Length);

            for(int i = 0; i < greyValues.Length; i++) {
                var cur = greyValues[i];
                if(i % dimentions == 0) {
                    Console.WriteLine();
                }

                var col = cur < 0.3 ? ConsoleColor.DarkRed : cur < 0.7 ? ConsoleColor.Red : ConsoleColor.Black;
                Console.ForegroundColor = col;
                Console.BackgroundColor = col;

                Console.Write(' ');
            }
        }

        private static bool IsImgRecogSuccess(double[] expected, double[] actual) {
            for(int i = 0; i < expected.Length; i++) {
                if(expected[i] == 1) {
                    var success = actual.Max() == actual[i];
                    return success;
                }
            }

            return false;
        }
    }
}