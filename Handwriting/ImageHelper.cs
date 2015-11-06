using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handwriting {

    public static class ImageHelper {

        public static Bitmap CropWhitespace(this Bitmap bmp) {
            int bitPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat);

            if(bitPerPixel != 24 && bitPerPixel != 32)
                throw new InvalidOperationException($"Invalid PixelFormat: {bitPerPixel}b");

            var bottom = 0;
            var left = bmp.Width;
            var right = 0;
            var top = bmp.Height;

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            unsafe
            {
                byte* dataPtr = (byte*)bmpData.Scan0;

                for(var y = 0; y < bmp.Height; y++) {
                    for(var x = 0; x < bmp.Width; x++) {
                        var rgbPtr = dataPtr + (x * (bitPerPixel / 8));

                        var b = rgbPtr[0];
                        var g = rgbPtr[1];
                        var r = rgbPtr[2];

                        byte? a = null;
                        if(bitPerPixel == 32) {
                            a = rgbPtr[3];
                        }

                        if(b != 0xFF || g != 0xFF || r != 0xFF || (a.HasValue && a.Value != 0xFF)) {
                            if(x < left)
                                left = x;

                            if(x >= right)
                                right = x + 1;

                            if(y < top)
                                top = y;

                            if(y >= bottom)
                                bottom = y + 1;
                        }
                    }

                    dataPtr += bmpData.Stride;
                }
            }
            bmp.UnlockBits(bmpData);

            if(left < right && top < bottom) {
                var width = right - left;
                var height = bottom - top;

                var croppedImg = bmp.Clone(new Rectangle(left, top, width, height), bmp.PixelFormat);
                return croppedImg;
            } else {
                return bmp; // Entire image should be cropped, it is empty
            }
        }

        public static Bitmap Square(this Bitmap image) {
            var dimentions = image.Width > image.Height ? image.Width : image.Height;

            var squareImg = new Bitmap(dimentions, dimentions, image.PixelFormat);

            using(var graphics = Graphics.FromImage(squareImg)) {
                graphics.Clear(Color.White);

                int x = (dimentions - image.Width) / 2;
                int y = (dimentions - image.Height) / 2;

                graphics.DrawImageUnscaled(image, x, y);
            }

            return squareImg;
        }

        public static Bitmap Resize(this Bitmap image, int width, int height, bool highQuality) {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height, image.PixelFormat);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using(var graphics = Graphics.FromImage(destImage)) {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = highQuality ? CompositingQuality.HighQuality : CompositingQuality.HighSpeed;
                graphics.InterpolationMode = highQuality ? InterpolationMode.HighQualityBicubic : InterpolationMode.Low;
                graphics.SmoothingMode = highQuality ? SmoothingMode.HighQuality : SmoothingMode.None;
                graphics.PixelOffsetMode = highQuality ? PixelOffsetMode.HighQuality : PixelOffsetMode.None;

                using(var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static double[] GreyValues(this Bitmap bmp, double min, double max) {
            int bitPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat);
            var greyVals = new double[bmp.Width * bmp.Height];

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            unsafe
            {
                byte* dataPtr = (byte*)bmpData.Scan0;

                for(var y = 0; y < bmp.Height; y++) {
                    for(var x = 0; x < bmp.Width; x++) {
                        var rgbPtr = dataPtr + (x * (bitPerPixel / 8));
                        int pixTotal = 0;

                        pixTotal += rgbPtr[0];
                        pixTotal += rgbPtr[1];
                        pixTotal += rgbPtr[2];

                        if(bitPerPixel == 32) {
                            pixTotal += rgbPtr[3];
                        }

                        //level is 0 <--> 255
                        int greyLevel = pixTotal / (bitPerPixel / 8);

                        greyVals[y * bmp.Height + x] = NeuralNet.MathHelper.ShiftRange(greyLevel, 0, 255, min, max);
                    }
                    dataPtr += bmpData.Stride;
                }
            }
            bmp.UnlockBits(bmpData);

            return greyVals;
        }
    }
}