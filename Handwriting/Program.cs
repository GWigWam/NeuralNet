using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting {

    public class Program {
        private const string dirLoc = "F:/Zooi/github/NeuralNet/Handwriting/data/img";

        private static void Main(string[] args) {
            var loadStart = Environment.TickCount;
            var imgs = ImageReader.ReadFromDir(dirLoc, true, true, true, 32);
            Console.WriteLine($"Done loading after {Environment.TickCount - loadStart}Ms");

            var greyCalcStart = Environment.TickCount;
            var asGreyVals = imgs.Select(img => img.GreyValues()).ToArray();
            Console.WriteLine($"Done calculating grey values after {Environment.TickCount - greyCalcStart}Ms");

            foreach(var val in asGreyVals.Skip(100).Take(7)) {
                PrintGreyValueImg(val);
                Console.WriteLine();
            }

            Console.ReadKey();
        }

        private static void PrintGreyValueImg(double[] greyValues) {
            int dimentions = (int)Math.Sqrt(greyValues.Length);

            for(int i = 0; i < greyValues.Length; i++) {
                var cur = greyValues[i];
                if(i % dimentions == 0) {
                    Console.WriteLine();
                }

                var col = cur < 0.3 ? ConsoleColor.DarkRed : cur < 0.7 ? ConsoleColor.Red : ConsoleColor.Black;
                Console.ForegroundColor = col;
                Console.BackgroundColor = col;

                Console.Write(' ');
            }
        }
    }
}