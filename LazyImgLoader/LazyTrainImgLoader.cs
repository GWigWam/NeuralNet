using BitmapHelper;
using NeuralNet;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LazyImgLoader {

    public class LazyTrainImgLoader {

        public int PreLoad {
            get; set;
        } = 100;

        public bool OnlyNumbers {
            get;
        }

        public bool CropWhitespace {
            get;
        }

        public bool HighQuality {
            get;
        }

        public int Dimensions {
            get;
        }

        public int BatchSize {
            get;
        }

        private Tuple<FileInfo, char>[] Files {
            get;
        }

        private Task PreLoadTask;

        private volatile ConcurrentBag<InputExpectedResult> PreLoaded;

        private volatile bool Loading;

        private volatile int curIndex = 0;

        private TransferFunction Transfer {
            get;
        }

        public int Index => curIndex;
        public int FileCount => Files.Length;

        public LazyTrainImgLoader(string dirLoc, TransferFunction transfer, bool cropWhitespace, bool highQuality, int dimensions, int batchSize, bool startPreLoad = true, bool onlyNumbers = true) {
            PreLoaded = new ConcurrentBag<InputExpectedResult>();
            Transfer = transfer;
            OnlyNumbers = onlyNumbers;
            CropWhitespace = cropWhitespace;
            HighQuality = highQuality;
            Dimensions = dimensions;
            BatchSize = batchSize;

            var random = new Random();
            Files = GenFileList(dirLoc)/*.OrderBy(e => random.Next())*/.ToArray();

            if(startPreLoad) {
                StartPreLoad();
            }
        }

        public InputExpectedResult[] GetNextBatch() {
            if(Loading) {
                PerformanceLog.LogSingle("Next batch requested, but not yet done loading", true, ConsoleColor.White, ConsoleColor.Red);
                PreLoadTask.Wait();
            }

            var ret = PreLoaded.ToArray(); //Collect result BEFORE callilng 'StartPreLoad'
            StartPreLoad();

            return ret;
        }

        public IEnumerable<InputExpectedResult> SimpleGet(int index, int size) {
            foreach(var file in Files.Skip(index).Take(size)) {
                yield return GenInOutPair(file);
            }
        }

        public void ResetIndex() {
            curIndex = 0;
            StartPreLoad();
        }

        private IEnumerable<Tuple<FileInfo, char>> GenFileList(string dirLoc) {
            var FileFormat = new Regex(@"_(?<char>[^_]+)_[A-Z0-9]+?.bmp$");

            var files = new DirectoryInfo(dirLoc).GetFiles("*.bmp", SearchOption.AllDirectories);

            foreach(var file in files) {
                string readChar = FileFormat.Match(file.Name).Groups["char"]?.Value;
                char curChar;
                if(char.TryParse(readChar, out curChar) && (!OnlyNumbers || char.IsNumber(curChar))) {
                    yield return new Tuple<FileInfo, char>(file, curChar);
                }
            }
        }

        private InputExpectedResult GenInOutPair(Tuple<FileInfo, char> data) {
            float[] inp = ImageReader.ReadImg(data.Item1.FullName, CropWhitespace, HighQuality, Dimensions).GreyValues(Transfer.ExtremeMin, Transfer.ExtremeMax);

            if(OnlyNumbers) {
                var nr = int.Parse(data.Item2.ToString());
                var outp = new float[10];
                for(int i = 0; i < outp.Length; i++) {
                    outp[i] = i == nr ? Transfer.ExtremeMax : Transfer.ExtremeMin;
                }

                return new InputExpectedResult(inp, outp);
            } else {
                throw new NotImplementedException();
            }
        }

        private void StartPreLoad() {
            Loading = true;
            PreLoaded = new ConcurrentBag<InputExpectedResult>();

            PreLoadTask = new Task(() => {
                var todoFiles = Files.Skip(curIndex);
                if(curIndex + BatchSize <= Files.Length) {
                    todoFiles = todoFiles.Take(BatchSize);
                }

                Parallel.ForEach(todoFiles, (cur) => {
                    var pair = GenInOutPair(cur);
                    PreLoaded.Add(pair);
                });
            });

            PreLoadTask.ContinueWith((task) => {
                if(task.IsCompleted) {
                    curIndex += BatchSize;

                    if(curIndex >= Files.Length) {
                        curIndex = 0;
                    }

                    Loading = false;
                }
            });

            PreLoadTask.Start();
        }
    }
}