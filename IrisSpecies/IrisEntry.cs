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
        public double SepalLength;
        public double SepalWidth;
        public double PetalLength;
        public double PetalWidth;
        public IrisSpecies Species;

        public double[] AsInput => new double[] { SepalLength, SepalWidth, PetalLength, PetalWidth };
        public double[] AsOutput => (Species == IrisSpecies.Setosa) ? new double[] { 1, 0, 0 } : (Species == IrisSpecies.Versicolor) ? new double[] { 0, 1, 0 } : new double[] { 0, 0, 1 };

        public IrisEntry(double sl, double sw, double pl, double pw, IrisSpecies species) {
            SepalLength = sl;
            SepalWidth = sw;
            PetalLength = pl;
            PetalWidth = pw;
            Species = species;
        }

        public static IrisSpecies SpeciesFromOutput(double[] output) {
            double maxVal = int.MinValue;
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

        public static bool IsOutputSuccess(double[] expected, double[] actual) {
            return SpeciesFromOutput(actual) == SpeciesFromOutput(expected);
        }
    }
}