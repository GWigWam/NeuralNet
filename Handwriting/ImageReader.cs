using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Handwriting {

    public static class ImageReader {
        private static Regex FileFormat;

        static ImageReader() {
            FileFormat = new Regex(@"_(?<char>[^_]+)_[A-Z0-9]+?.bmp$");
        }

        public static void ReadFromDir(string dirLoc, bool onlyNumbers = true) {
            foreach(FileInfo file in new DirectoryInfo(dirLoc).GetFiles("*.bmp", SearchOption.AllDirectories)) {
                string readChar = FileFormat.Match(file.Name).Groups["char"]?.Value;
                char curChar;
                if(char.TryParse(readChar, out curChar) && (!onlyNumbers || char.IsNumber(curChar))) {
                }
            }
        }
    }
}