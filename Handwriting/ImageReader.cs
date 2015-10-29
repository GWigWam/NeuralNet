using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Handwriting {

    public static class ImageReader {
        private static Regex FileFormat;

        /*        static ImageReader() {
                    FileFormat = new Regex(@"_(?<char>[^_]+)_[A-Z0-9]+?.bmp$");
                }

                public static List<Bitmap> ReadFromDir(string dirLoc, bool onlyNumbers, bool cropWhitespace, bool highQuality, int dimentions) {
                    var imgs = new List<Bitmap>();
                    var files = new DirectoryInfo(dirLoc).GetFiles("*.bmp", SearchOption.AllDirectories);

                    Parallel.ForEach(files, (file) => {
                        string readChar = FileFormat.Match(file.Name).Groups["char"]?.Value;
                        char curChar;
                        if(char.TryParse(readChar, out curChar) && (!onlyNumbers || char.IsNumber(curChar))) {
                            Bitmap img = (Bitmap)Image.FromFile(file.FullName);

                            if(cropWhitespace)
                                img = img.CropWhitespace();

                            img = img.Square();
                            img = img.Resize(dimentions, dimentions, highQuality);

                            imgs.Add(img);
                        }
                    });

                    return imgs;
                }*/

        public static Bitmap ReadImg(string imgLoc, bool cropWhitespace, bool highQuality, int dimentions) {
            Bitmap img = (Bitmap)Image.FromFile(imgLoc);

            if(cropWhitespace)
                img = img.CropWhitespace();

            img = img.Square();
            img = img.Resize(dimentions, dimentions, highQuality);

            return img;
        }
    }
}