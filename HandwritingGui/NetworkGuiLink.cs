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

        public StatsOverTimeModel StatsOverTime { get; private set; }

        public NetworkGuiLink() {
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
            InitNetwork(learnRate, microBatchsize, inputHeight, outputHeight, hiddenHeights);
        }

        private void InitNetwork(double learnRate, int microBatchsize, int inputHeight, int outputHeight, int[] hiddenHeights) {
            Network = new Network(Transfer, true);
            Network.FillNetwork(inputHeight, outputHeight, hiddenHeights);
            BackpropTrain = new Backpropagate(Network, learnRate, microBatchsize);
        }
    }
}