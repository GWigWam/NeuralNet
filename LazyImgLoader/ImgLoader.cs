using BitmapHelper;
using NeuralNet;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImgLoader {
    public abstract class ImgLoader {
        public bool LoadNumbers { get; }
        public bool LoadLowerChars { get; }
        public bool LoadUpperChars { get; }
        public bool CropWhitespace { get; }
        public bool HighQuality { get; }
        public int BatchSize { get; }
        public int Dimensions { get; }
        public TransferFunction Transfer { get; }

        protected ImgFile[] Files { get; }
        protected Dictionary<char, int> CharNumMappings { get; } = new Dictionary<char, int>();

        public int FileCount => Files.Length;

        public abstract int Index { get; }
        
        public ImgLoader(string dirLoc, TransferFunction transfer, bool cropWhitespace, bool highQuality, int dimensions, int batchSize, bool nrs, bool lower, bool upper) {
            Transfer = transfer;
            CropWhitespace = cropWhitespace;
            HighQuality = highQuality;
            BatchSize = batchSize;
            Dimensions = dimensions;
            LoadNumbers = nrs;
            LoadLowerChars = lower;
            LoadUpperChars = upper;

            Files = GenFileList(dirLoc).ToArray();

            InitCharNumMappings();
        }

        public abstract IEnumerable<InputExpectedResult> SimpleGet(int index, int size);
        public abstract IEnumerable<InputExpectedResult> GetNextBatch();

        protected IEnumerable<ImgFile> GenFileList(string dirLoc) {
            var FileFormat = new Regex(@"_(?<char>[^_]+)_[A-Z0-9]+?.bmp$");

            var files = new DirectoryInfo(dirLoc).GetFiles("*.bmp", SearchOption.AllDirectories);

            foreach(var file in files) {
                string readChar = FileFormat.Match(file.Name).Groups["char"]?.Value;
                if(char.TryParse(readChar, out var curChar) && (
                    (LoadNumbers && char.IsNumber(curChar)) ||
                    (LoadLowerChars && char.IsLetter(curChar) && char.IsLower(curChar)) ||
                    (LoadUpperChars && char.IsLetter(curChar) && char.IsUpper(curChar)))) {

                    yield return new ImgFile(file.FullName, curChar);
                }
            }
        }

        protected InputExpectedResult GenInOutPair(ImgFile img) {
            float[] inp = ImageReader.ReadImg(img.Path, CropWhitespace, HighQuality, Dimensions).GreyValues(Transfer.ExtremeMin, Transfer.ExtremeMax);

            var nr = CharNumMappings[img.Char];
            var outp = new float[(LoadNumbers ? 10 : 0) + (LoadLowerChars ? 26 : 0) + (LoadUpperChars ? 26 : 0)];
            for(int i = 0; i < outp.Length; i++) {
                outp[i] = i == nr ? Transfer.ExtremeMax : Transfer.ExtremeMin;
            }

            return new InputExpectedResult(inp, outp);
        }

        private void InitCharNumMappings() {
            void add(char start, int count) {
                for(int i = start; i < start + count; i++) {
                    CharNumMappings[(char)i] = CharNumMappings.Count;
                }
            }
            if(LoadNumbers) {
                add('0', 10);
            }
            if(LoadLowerChars) {
                add('a', 26);
            }
            if(LoadUpperChars) {
                add('A', 26);
            }
        }

        protected struct ImgFile {
            public string Path { get; }
            public char Char { get; }

            public ImgFile(string path, char @char) {
                Path = path;
                Char = @char;
            }
        }
    }
}
