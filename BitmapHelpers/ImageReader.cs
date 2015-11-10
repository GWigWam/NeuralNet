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

namespace BitmapHelper {

    public static class ImageReader {

        public static Bitmap ReadImg(string imgLoc, bool cropWhitespace, bool highQuality, int dimensions) {
            Bitmap img = (Bitmap)Image.FromFile(imgLoc);

            if(cropWhitespace)
                img = img.CropWhitespace();

            img = img.Square();
            img = img.Resize(dimensions, dimensions, highQuality);

            return img;
        }
    }
}