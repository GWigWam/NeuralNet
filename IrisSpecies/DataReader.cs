﻿using System;
using System.Collections.Generic;
using System.Globalization;
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
                if(split.Length == 5) {
                    float sepalLength = float.Parse(split[0], CultureInfo.InvariantCulture);
                    float sepalWidth = float.Parse(split[1], CultureInfo.InvariantCulture);
                    float petalLength = float.Parse(split[2], CultureInfo.InvariantCulture);
                    float petalWidth = float.Parse(split[3], CultureInfo.InvariantCulture);

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
            }

            return readEntries.ToArray();
        }
    }
}