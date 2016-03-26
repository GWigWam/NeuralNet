using HandwritingGui.PlotModels;
using LazyImgLoader;
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

        public LazyTrainImgLoader ImgLoader {
            get; private set;
        }

        public TransferFunction TransferFunc {
            get; private set;
        }

        public Network Network {
            get; private set;
        }

        public Backpropagate BackpropTrain {
            get; private set;
        }

        private volatile bool Train = false;
        private volatile Task TrainingTask;
        public bool IsTraining => TrainingTask != null && Train;

        public event PropertyChangedEventHandler PropertyChanged;

        public StatsOverTimeModel StatsOverTime { get; private set; }
        public int ImgCount => ImgLoader.FileCount;
        public int CurImgIndex => ImgLoader.Index;

        public NetworkGuiLink() {
            StatsOverTime = new StatsOverTimeModel();
        }

        public int ImageDimensions {
            get; private set;
        }

        private TaskFactory taskFactory {
            get; set;
        }

        public void Init(int imgDim, double learnRate, int microBatchsize, int loadBatchsize, string imgFolder, TransferFunctionType funcType, int inputHeight, int outputHeight, int[] hiddenHeights) {
            taskFactory = new TaskFactory();

            switch(funcType) {
                case TransferFunctionType.Sigmoid:
                TransferFunc = new SigmoidFunction();
                break;

                case TransferFunctionType.HyperbolicTangent:
                TransferFunc = new HyperbolicTangentFunction();
                break;
            }
            ImageDimensions = imgDim;

            ImgLoader = new LazyTrainImgLoader(imgFolder, TransferFunc, true, true, imgDim, loadBatchsize/*, TODO: Support non-digits*/);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImgCount"));
            InitNetwork(learnRate, microBatchsize, inputHeight, outputHeight, hiddenHeights);
        }

        public void StartTraining() {
            if(TrainingTask == null && !Train) {
                Train = true;
                TrainingTask = taskFactory.StartNew(TrainLoop);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsTraining"));
            }
        }

        public async void PauseTraining() {
            await Task.Run(() => {
                if(TrainingTask != null && Train) {
                    Train = false;
                    TrainingTask.Wait();
                    TrainingTask = null;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsTraining"));
                }
            });
        }

        private void InitNetwork(double learnRate, int microBatchsize, int inputHeight, int outputHeight, int[] hiddenHeights) {
            Network = new Network(TransferFunc, true);
            Network.FillNetwork(inputHeight, outputHeight, hiddenHeights);
            BackpropTrain = new Backpropagate(Network, learnRate, microBatchsize);
        }

        private void TrainLoop() {
            while(Train) {
                var trainData = ImgLoader.GetNextBatch();

                BackpropTrain.Train(trainData);

                var stats = NetworkValidation.Validate(Network, trainData, IsImgRecogSuccess);
                StatsOverTime.AddBoth(stats.AvgSSE, stats.SuccessPercentage);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurImgIndex"));
            }
        }

        private static bool IsImgRecogSuccess(double[] expected, double[] actual) {
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