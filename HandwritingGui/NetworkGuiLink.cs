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
using System.Threading;
using System.Threading.Tasks;

namespace HandwritingGui {

    internal class NetworkGuiLink : INotifyPropertyChanged, IDisposable {

        public LazyTrainImgLoader ImgLoader {
            get; private set;
        }

        public TransferFunction TransferFunc {
            get; private set;
        }

        public Network2 Network {
            get; private set;
        }

        public Backpropagate2 BackpropTrain {
            get; private set;
        }

        private object trainLock = new object();
        private volatile bool train = false;

        private bool Train {
            get { lock (trainLock) { return train; } }
            set { lock (trainLock) { train = value; } }
        }

        private volatile Thread TrainThread;
        public bool IsTraining => Train;

        public event PropertyChangedEventHandler PropertyChanged;

        public StatsOverTimeModel StatsOverTime { get; private set; }
        public int ImgCount => ImgLoader?.FileCount ?? -1;
        public int CurImgIndex => ImgLoader?.Index ?? -1;

        public NetworkGuiLink() {
            StatsOverTime = new StatsOverTimeModel();
        }

        public int ImageDimensions {
            get; private set;
        }

        public int MicroBatchSize { get; set; }

        public void Init(int imgDim, double learnRate, int microBatchsize, int loadBatchsize, string imgFolder, TransferFunctionType funcType, int inputHeight, int outputHeight, int[] hiddenHeights, bool nums, bool lower, bool upper) {
            switch(funcType) {
                case TransferFunctionType.Sigmoid:
                TransferFunc = new SigmoidFunction();
                break;

                case TransferFunctionType.HyperbolicTangent:
                TransferFunc = new HyperbolicTangentFunction();
                break;
            }
            ImageDimensions = imgDim;
            MicroBatchSize = microBatchsize;

            ImgLoader = new LazyTrainImgLoader(imgFolder, TransferFunc, true, true, imgDim, loadBatchsize, nums, lower, upper);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImgCount"));
            InitNetwork(learnRate, inputHeight, outputHeight, hiddenHeights);
        }

        private void InitNetwork(double learnRate, int inputHeight, int outputHeight, int[] hiddenHeights) {
            //Network = new Network(TransferFunc, true);
            Network = new Network2(TransferFunc);
            Network.FillNetwork(inputHeight, outputHeight, hiddenHeights);
            //BackpropTrain = new Backpropagate(Network, learnRate, microBatchsize);
            BackpropTrain = new Backpropagate2(Network, (float)learnRate);
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

        public void StartTraining() {
            if(TrainThread == null) {
                TrainThread = new Thread(new ThreadStart(TrainLoop));
                TrainThread.Priority = ThreadPriority.Highest;
                TrainThread.Start();
            }
            if(!Train) {
                Train = true;
            }
        }

        public void PauseTraining() {
            Train = false;
        }

        private void TrainLoop() {
            var validationData = ImgLoader.SimpleGet(0, 500).ToArray();
            bool doTrain = Train;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsTraining"));
            while(true) {
                if(doTrain) {
                    var trainData = ImgLoader.GetNextBatch();

                    BackpropTrain.Train(trainData, MicroBatchSize);

                    var stats = NetworkValidation.Validate(Network, validationData, IsImgRecogSuccess);
                    StatsOverTime.AddBoth(stats.AvgSSE, stats.SuccessPercentage);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurImgIndex"));

                    if(!Train) {
                        doTrain = false;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsTraining"));
                    }
                } else {
                    if(Train) {
                        doTrain = true;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsTraining"));
                    } else {
                        Thread.Sleep(100);
                    }
                }
            }
        }

        public void Dispose() {
            TrainThread?.Abort();
        }
    }
}