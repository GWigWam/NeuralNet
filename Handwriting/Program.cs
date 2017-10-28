using LazyImgLoader;
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
        private static readonly Random random = new Random();

        //Transfer function
        private static readonly TransferFunction transfer = new HyperbolicTangentFunction();

        //Consts
        private static readonly int[] HiddenLayerHeights = new int[] { 30 };

        private const int ImgDimensions = 16;
        private const double LearningRate = 0.001;
        private const int MicroBatchSize = 10;
        private const int LoadBatchSize = 300;

        private const string dirLoc = "E:/Handwriting Data/HSF_0_SUB/";

        private static void Main(string[] args) {
            LogSingle("Start", false, ConsoleColor.White, ConsoleColor.Red);
            var imgLoader = new LazyTrainImgLoader(dirLoc, transfer, true, true, ImgDimensions, LoadBatchSize, true, true);

            InputExpectedResult[] validateSet = imgLoader.GetNextBatch();

            LogSingle("ImageLoader read, create & train network", true);
            var network = new Network(transfer, true);
            network.FillNetwork(ImgDimensions * ImgDimensions, 10, HiddenLayerHeights);

            var backpropTraining = new Backpropagate(network, LearningRate, MicroBatchSize);

            var startTime = Environment.TickCount;
            double prevSSE = double.MaxValue;
            InputExpectedResult[] trainData;
            int epoch = 0;
            double CurLearnRate = LearningRate;
            while((trainData = imgLoader.GetNextBatch()).Length > 0) {
                backpropTraining.Train(trainData.OrderBy(e => random.Next()).ToArray());

                var stats = NetworkValidation.Validate(network, trainData, IsImgRecogSuccess);
                LogSingle($"{imgLoader.Index,-4} / {imgLoader.FileCount,-5} | LearnRate: {backpropTraining.LearningRate,-8} | " + stats.ToString(), false);
                PrintProcesses(true);

                if(imgLoader.Index >= imgLoader.FileCount - imgLoader.BatchSize) {
                    epoch++;
                    LogSingle($"Done epoch #{epoch}", false, ConsoleColor.Green);

                    var validateStats = NetworkValidation.Validate(network, validateSet, IsImgRecogSuccess);
                    LogSingle("VALIDATION SET: " + validateStats.ToString(), true, ConsoleColor.Green);
                    LogSingle($"Total time: {Environment.TickCount - startTime}Ms", false, ConsoleColor.Green);

                    if(prevSSE <= validateStats.AvgSSE) {
                        CurLearnRate /= 2;
                        backpropTraining.LearningRate = CurLearnRate;
                    }
                    prevSSE = validateStats.AvgSSE;
                }
            }

            Console.ReadKey();
        }

        private static void PrintGreyValueImg(double[] greyValues) {
            var max = transfer.ExtremeMax;
            var min = transfer.ExtremeMin;
            var range = max - min;

            var startForeColor = Console.ForegroundColor;
            var startBackColor = Console.BackgroundColor;

            int dimensions = (int)Math.Sqrt(greyValues.Length);

            for(int i = 0; i < greyValues.Length; i++) {
                var cur = greyValues[i];
                if(i % dimensions == 0) {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine();
                }

                var col = cur < (min + range / 7) ? ConsoleColor.DarkRed : cur < (min + range / 3) ? ConsoleColor.Red : ConsoleColor.Black;
                Console.ForegroundColor = col;
                Console.BackgroundColor = col;

                Console.Write(' ');
            }

            Console.ForegroundColor = startForeColor;
            Console.BackgroundColor = startBackColor;
        }

        private static bool IsImgRecogSuccess(float[] expected, float[] actual) {
            var max = actual.Max();
            if(actual.Count(d => d == max) == 1) {
                for(int i = 0; i < expected.Length; i++) {
                    if(expected[i] == 1) {
                        var success = max == actual[i];
                        return success;
                    }
                }
            }

            return false;
        }
    }
}