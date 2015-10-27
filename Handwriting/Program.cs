using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting {

    public class Program {
        private const string dirLoc = "F:/Zooi/github/NeuralNet/Handwriting/data/img";

        private static void Main(string[] args) {
            ImageReader.ReadFromDir(dirLoc);
        }
    }
}