using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrisSpecies {

    internal enum IrisSpecies {
        Setosa, Versicolor, Virginica
    }

    internal struct IrisEntry {
        public float SepalLength;
        public float SepalWidth;
        public float PetalLength;
        public float PetalWidth;
        public IrisSpecies Species;

        public IrisEntry(float sl, float sw, float pl, float pw, IrisSpecies species) {
            SepalLength = sl;
            SepalWidth = sw;
            PetalLength = pl;
            PetalWidth = pw;
            Species = species;
        }
    }
}