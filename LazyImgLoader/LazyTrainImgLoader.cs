using BitmapHelper;
using NeuralNet;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImgLoader {    
    public class LazyTrainImgLoader : ImgLoader {
        
        private Task PreLoadTask;

        private ConcurrentBag<InputExpectedResult> PreLoaded;

        private bool Loading;

        private int curIndex = 0;
        public override int Index => curIndex;

        public LazyTrainImgLoader(string dirLoc, TransferFunction transfer, bool cropWhitespace, bool highQuality, int dimensions, int batchSize, bool nrs, bool lower, bool upper, bool startPreLoad = true)
            : base(dirLoc, transfer, cropWhitespace, highQuality, dimensions, batchSize, nrs, lower, upper) {

            PreLoaded = new ConcurrentBag<InputExpectedResult>();

            if(startPreLoad) {
                StartPreLoad();
            }
        }

        public override IEnumerable<InputExpectedResult> GetNextBatch() {
            if(Loading) {
                PerformanceLog.LogSingle("Next batch requested, but not yet done loading", true, ConsoleColor.White, ConsoleColor.Red);
                PreLoadTask.Wait();
            }

            var ret = PreLoaded.ToArray(); //Collect result BEFORE callilng 'StartPreLoad'
            StartPreLoad();

            return ret;
        }

        public override IEnumerable<InputExpectedResult> SimpleGet(int index, int size) {
            foreach(var file in Files.Skip(index).Take(size)) {
                yield return GenInOutPair(file);
            }
        }

        public void ResetIndex() {
            curIndex = 0;
            StartPreLoad();
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
