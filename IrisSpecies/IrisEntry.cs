using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrisSpecies {

    internal enum IrisSpecies {
        Setosa, Versicolor, Virginica
    }

    internal class IrisEntry {
        public float SepalLength;
        public float SepalWidth;
        public float PetalLength;
        public float PetalWidth;
        public IrisSpecies Species;

        public float[] AsInput => new float[] { SepalLength, SepalWidth, PetalLength, PetalWidth };
        public float[] AsOutput => (Species == IrisSpecies.Setosa) ? new float[] { 1, 0, 0 } : (Species == IrisSpecies.Versicolor) ? new float[] { 0, 1, 0 } : new float[] { 0, 0, 1 };

        public IrisEntry(float sl, float sw, float pl, float pw, IrisSpecies species) {
            SepalLength = sl;
            SepalWidth = sw;
            PetalLength = pl;
            PetalWidth = pw;
            Species = species;
        }

        public static IrisSpecies SpeciesFromOutput(float[] output) {
            float maxVal = int.MinValue;
            int maxIndex = -1;
            for(int i = 0; i < output.Length; i++) {
                var curOutp = output[i];
                if(curOutp > maxVal) {
                    maxVal = curOutp;
                    maxIndex = i;
                }
            }

            switch(maxIndex) {
                case 0:
                return IrisSpecies.Setosa;

                case 1:
                return IrisSpecies.Versicolor;

                case 2:
                return IrisSpecies.Virginica;

                default:
                throw new Exception();
            }
        }

        public static bool IsOutputSuccess(float[] expected, float[] actual) {
            return SpeciesFromOutput(actual) == SpeciesFromOutput(expected);
        }
    }
}