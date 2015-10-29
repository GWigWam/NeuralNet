using NeuralNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Handwriting {

    internal class LazyTrainImgLoader {

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

        public int Dimentions {
            get;
        }

        public int BatchSize {
            get;
        }

        private Tuple<FileInfo, char>[] Files {
            get;
        }

        private Task PreLoadTask;

        private volatile List<InputExpectedResult> PreLoaded;

        private volatile bool Loading;

        private volatile int curIndex = 0;

        public int Index => curIndex;
        public int FileCount => Files.Length;

        public LazyTrainImgLoader(string dirLoc, bool onlyNumbers, bool cropWhitespace, bool highQuality, int dimentions, int batchSize) {
            PreLoaded = new List<InputExpectedResult>();
            OnlyNumbers = onlyNumbers;
            CropWhitespace = cropWhitespace;
            HighQuality = highQuality;
            Dimentions = dimentions;
            BatchSize = batchSize;

            Files = GenFileList(dirLoc).ToArray();

            StartPreLoad();
        }

        public InputExpectedResult[] GetNextBatch() {
            if(Loading) {
                Console.WriteLine("Next batch requested, but not yet done loading");
                PreLoadTask.Wait();
            }

            var ret = PreLoaded.ToArray(); //Collect result BEFORE callilng 'StartPreLoad'
            StartPreLoad();

            return ret;
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
            double[] inp = ImageReader.ReadImg(data.Item1.FullName, CropWhitespace, HighQuality, Dimentions).GreyValues();

            if(OnlyNumbers) {
                var nr = int.Parse(data.Item2.ToString());
                var outp = new double[10];
                outp[nr] = 1;

                return new InputExpectedResult(inp, outp);
            } else {
                throw new NotImplementedException();
            }
        }

        private void StartPreLoad() {
            Loading = true;
            PreLoaded.Clear();

            PreLoadTask = new Task(() => {
                if(curIndex <= Files.Length) {
                    var todoFiles = Files.Skip(curIndex);
                    if(curIndex + BatchSize <= Files.Length) {
                        todoFiles = todoFiles.Take(BatchSize);
                    }

                    Parallel.ForEach(todoFiles, (cur) => {
                        var pair = GenInOutPair(cur);
                        PreLoaded.Add(pair);
                    });
                }
            });

            PreLoadTask.ContinueWith((task) => {
                if(task.IsCompleted) {
                    curIndex += BatchSize;

                    if(curIndex >= Files.Length) {
                        curIndex = Files.Length;
                    }

                    Loading = false;
                }
            });

            PreLoadTask.Start();
        }
    }
}