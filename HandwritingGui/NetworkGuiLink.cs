using HandwritingGui.PlotModels;
using NeuralNet;
using NeuralNet.BackpropagationTraining;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandwritingGui {

    internal class NetworkGuiLink : INotifyPropertyChanged {
        private LazyTrainImgLoader ImgLoader;
        private TransferFunction Transfer;
        private Network Network;
        private Backpropagate BackpropTrain;

        private Random RNG;
        private volatile bool Training = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public StatsOverTimeModel StatsOverTime { get; private set; }
        public int ImgCount => ImgLoader.FileCount;
        public int CurImgIndex => ImgLoader.Index;

        public NetworkGuiLink() {
            RNG = new Random();
            StatsOverTime = new StatsOverTimeModel();
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImgCount"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurImgIndex"));
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