using HandwritingGui.PlotModels;
using NeuralNet;
using NeuralNet.BackpropagationTraining;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandwritingGui {

    internal class NetworkGuiLink {
        private LazyTrainImgLoader ImgLoader;
        private TransferFunction Transfer;
        private Network Network;
        private Backpropagate BackpropTrain;

        private Random RNG;
        private volatile bool Training = false;

        public StatsOverTimeModel StatsOverTime { get; private set; }
        public SSEHistModel SSEHist { get; private set; }

        public NetworkGuiLink() {
            RNG = new Random();
            StatsOverTime = new StatsOverTimeModel();
            SSEHist = new SSEHistModel();
        }

        public void Init(int imgDim, double learnRate, int microBatchsize, int loadBatchsize, string imgFolder, TransferFunctionType funcType, int inputHeight, int outputHeight, int[] hiddenHeights) {
            switch(funcType) {
                case TransferFunctionType.Sigmoid:
                Transfer = new SigmoidFunction();
                break;

                case TransferFunctionType.HyperbolicTangent:
                Transfer = new HyperbolicTangentFunction();
                break;
            }

            ImgLoader = new LazyTrainImgLoader(imgFolder, Transfer, true, true, imgDim, loadBatchsize/*, TODO: Support non-digits*/);
            InitNetwork(learnRate, microBatchsize, inputHeight, outputHeight, hiddenHeights);
        }

        public void StartTraining() {
            Training = true;
            Task.Run(() => TrainLoop());
        }

        private void InitNetwork(double learnRate, int microBatchsize, int inputHeight, int outputHeight, int[] hiddenHeights) {
            Network = new Network(Transfer, true);
            Network.FillNetwork(inputHeight, outputHeight, hiddenHeights);
            BackpropTrain = new Backpropagate(Network, learnRate, microBatchsize);
        }

        private void TrainLoop() {
            while(Training) {
                var trainData = ImgLoader.GetNextBatch();
                if(trainData.Length < 1) {
                    //End of epoch
                    ImgLoader.ResetIndex();
                    trainData = ImgLoader.GetNextBatch();
                }

                BackpropTrain.Train(trainData.OrderBy(e => RNG.Next()).ToArray());

                var stats = NetworkValidation.Validate(Network, trainData, IsImgRecogSuccess);
                StatsOverTime.AddBoth(stats.AvgSSE, stats.SuccessPercentage);
                SSEHist.Update(stats.GetSSEs());
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