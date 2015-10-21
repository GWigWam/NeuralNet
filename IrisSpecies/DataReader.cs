using System;
using System.Collections.Generic;
using System.IO;

namespace IrisSpecies {

    internal static class DataReader {

        public static IrisEntry[] ReadFromFile(string fileLocation) {
            var readEntries = new List<IrisEntry>(150);
            string fullText;
            using(var sr = new StreamReader(fileLocation)) {
                fullText = sr.ReadToEnd();
            }

            foreach(string entry in fullText.Split('\n')) {
                var split = entry.Split(',');
                float sepalLength = float.Parse(split[0]);
                float sepalWidth = float.Parse(split[1]);
                float petalLength = float.Parse(split[2]);
                float petalWidth = float.Parse(split[3]);

                IrisSpecies species;
                if(split[4].StartsWith("Iris-s")) {
                    species = IrisSpecies.Setosa;
                } else if(split[4].StartsWith("Iris-ve")) {
                    species = IrisSpecies.Versicolor;
                } else if(split[4].StartsWith("Iris-vi")) {
                    species = IrisSpecies.Virginica;
                } else {
                    throw new Exception("Unknown iris species");
                }

                readEntries.Add(new IrisEntry(sepalLength, sepalWidth, petalLength, petalWidth, species));
            }

            return readEntries.ToArray();
        }
    }
}