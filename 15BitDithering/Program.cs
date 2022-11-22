using System;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _15BitDithering
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if(args.Length <= 0)
            {
                Console.WriteLine("Missing input parameter.");
                Console.WriteLine($"Usage: {AppDomain.CurrentDomain.FriendlyName} input.png [input2.png ...]");
                return;
            }

            foreach(var arg in args)
            {
                if(arg.EndsWith(".png") && File.Exists(arg))
                {
                    Dither(arg);
                }
                else if(Directory.Exists(arg))
                {
                    var dir = Path.GetDirectoryName(arg);
                    var files = Directory.GetFiles(arg, "*.png", SearchOption.AllDirectories);
                    foreach(var file in files)
                    {
                        var dir2 = Path.GetDirectoryName(file);
                        var aaaa = dir2.Substring(dir.Length+1); // :)
                        Dither(file, aaaa);
                    }
                }
            }
        }

        static void Dither(string path, string directory = null)
        {
            var fileName = Path.GetFileName(path);
            
            var png = new PngBitmapDecoder(new Uri(path), BitmapCreateOptions.None, BitmapCacheOption.None);
            var frame = png.Frames[0];
            int height = frame.PixelHeight;
            int width = frame.PixelWidth;
            int stride = width * ((frame.Format.BitsPerPixel + 7) / 8);

            byte[] bits = new byte[height * stride];

            frame.CopyPixels(bits, stride, 0);
            var dat = RGB555Dithered(bits, width, height, stride);

            var outimg = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr555, null, dat, width * ((PixelFormats.Bgr555.BitsPerPixel + 7) / 8));
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(outimg));

            var outputDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Output", directory);

            Directory.CreateDirectory(outputDir);
            using (var fstream = new FileStream(Path.Combine(outputDir, fileName), FileMode.Create, FileAccess.Write))
                enc.Save(fstream);
            Console.WriteLine($"Saved {Path.Combine(outputDir, fileName)}");
        }

        static byte Clamp(int n)
        {
            n = n > 255 ? 255 : n;
            return (byte)(n < 0 ? 0 : n);
        }

        struct RGBTriplet
        {
            public int r;
            public int g;
            public int b;
            RGBTriplet(int _r = 0, int _g = 0, int _b = 0)
            {
                r = _r;
                g = _g;
                b = _b;
            }
        };


        // Code by Mark Ransom (https://stackoverflow.com/a/11650801) - Modified for 15-bit and translated to C#
        static byte[] RGB555Dithered(byte[] pIn, int width, int height, int strideIn)
        {
            int strideOut = width * ((PixelFormats.Bgr555.BitsPerPixel + 7) / 8);
            var pOut = new byte[strideOut * height];
            RGBTriplet[] oldErrors = new RGBTriplet[width + 2];
            for (int y = 0; y < height; ++y)
            {
                RGBTriplet[] newErrors = new RGBTriplet[width + 2];
                RGBTriplet errorAhead = new RGBTriplet();
                for (int x = 0; x < width; ++x)
                {
                    int b = (int)(byte)pIn[3 * x + y * strideIn] + (errorAhead.b + oldErrors[x + 1].b) / 16;
                    int g = (int)(byte)pIn[3 * x + 1 + y * strideIn] + (errorAhead.g + oldErrors[x + 1].g) / 16;
                    int r = (int)(byte)pIn[3 * x + 2 + y * strideIn] + (errorAhead.r + oldErrors[x + 1].r) / 16;
                    int bAfter = Clamp(b) >> 3;
                    int gAfter = Clamp(g) >> 3;
                    int rAfter = Clamp(r) >> 3;
                    int pixel16 = (rAfter << 10) | (gAfter << 5) | bAfter;
                    pOut[2 * x + y * strideOut] = (byte)pixel16;
                    pOut[2 * x + 1 + y * strideOut] = (byte)(pixel16 >> 8);
                    int error = r - ((rAfter * 255) / 31);
                    errorAhead.r = error * 7;
                    newErrors[x].r += error * 3;
                    newErrors[x + 1].r += error * 5;
                    newErrors[x + 2].r = error * 1;
                    error = g - ((gAfter * 255) / 31);
                    errorAhead.g = error * 7;
                    newErrors[x].g += error * 3;
                    newErrors[x + 1].g += error * 5;
                    newErrors[x + 2].g = error * 1;
                    error = b - ((bAfter * 255) / 31);
                    errorAhead.b = error * 7;
                    newErrors[x].b += error * 3;
                    newErrors[x + 1].b += error * 5;
                    newErrors[x + 2].b = error * 1;
                }
                oldErrors = newErrors;
            }
            return pOut;
        }
    }
}
