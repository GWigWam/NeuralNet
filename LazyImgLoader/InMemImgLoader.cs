using NeuralNet;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgLoader {
    public class InMemImgLoader : ImgLoader {

        private int index;
        public override int Index { get => index; }

        private InputExpectedResult[] Data { get; set; }

        public InMemImgLoader(string dirLoc, TransferFunction transfer, bool cropWhitespace, bool highQuality, int dimensions, int batchSize, bool nrs, bool lower, bool upper)
            : base(dirLoc, transfer, cropWhitespace, highQuality, dimensions, batchSize, nrs, lower, upper) { }

        public void Init() {
            Data = Load().ToArray();
        }

        private IEnumerable<InputExpectedResult> Load() => Files.AsParallel().Select(f => GenInOutPair(f));

        public override IEnumerable<InputExpectedResult> GetNextBatch() {
            var res = SimpleGet(index, BatchSize);
            index = index >= Data.Length ? 0 : index + BatchSize;
            return res;
        }

        public override IEnumerable<InputExpectedResult> SimpleGet(int index, int size) {
            for(int i = index; i < index + size && i < Data.Length; i++) {
                yield return Data[i];
            }
        }
    }
}
