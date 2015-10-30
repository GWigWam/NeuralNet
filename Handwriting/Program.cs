﻿using NeuralNet;
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
        private const int dimentions = 28;

        private static void Main(string[] args) {
            LogSingle("Start");
            var imgLoader = new LazyTrainImgLoader(dirLoc, true, true, true, dimentions, 100);

            LogSingle("ImageLoader read, create & train network");
            var network = new Network(new SigmoidFunction(), true);
            network.FillNetwork(dimentions * dimentions, 10, 15);
            //network.RandomizeWeights();

            var backpropTraining = new Backpropagate(network, 3, 10);

            InputExpectedResult[] trainData;
            while((trainData = imgLoader.GetNextBatch()).Length > 0) {
                backpropTraining.Train(trainData);

                var stats = NetworkValidation.Validate(network, trainData, IsImgRecogSuccess);
                LogSingle($"{imgLoader.Index,-4} / {imgLoader.FileCount,-5} | " + stats.ToString());
                PrintProcesses(true);
            }
            LogSingle("Done training");

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