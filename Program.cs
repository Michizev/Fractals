using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Fractals
{
    struct Fractal
    {
        public Fractal(int x, int y, int count)
        {
            X = x;
            Y = y;
            Count = count;
        }
        public int X { get; }
        public int Y { get; }
        public int Count { get; }
    }
    class FractalMaker
    {
        public FractalData GenerateFractals(float left, float top, float xside, float yside, int maxX, int maxY, int maxCount)
        {
            List<Fractal> pos = new List<Fractal>();
            float xscale, yscale, zx, zy, cx, tempx, cy;
            int count;

            // setting up the xscale and yscale 
            xscale = (float)xside / maxX;
            yscale = (float)yside / maxY;

            for (int y = 0; y < maxY; y++)
            {
                for (int x = 0; x < maxX; x++)
                {
                    // c_real 
                    cx = x * xscale + left;
                    // c_imaginary 
                    cy = y * yscale + top;
                    // z_real 
                    zx = 0;
                    // z_imaginary 
                    zy = 0;

                    count = 0;

                    if (x < 4 && y == 0)
                    {
                        Console.WriteLine(cx);
                    }

                    // Calculate whether c(c_real + c_imaginary) belongs 
                    // to the Mandelbrot set or not and draw a pixel 
                    // at coordinates (x, y)
                    while ((zx * zx + zy * zy < 4) && (count < maxCount))
                    {
                        // Calculate Mandelbrot function 
                        // z = z*z + c where z is a complex number 

                        // tempx = z_real*_real - z_imaginary*z_imaginary + c_real 
                        tempx = zx * zx - zy * zy + cx;

                        // 2*z_real*z_imaginary + c_imaginary 
                        zy = 2 * zx * zy + cy;

                        // Updating z_real = tempx 
                        zx = tempx;

                        // Increment count 
                        ++count;
                    }
                    // To display the created fractal 
                    pos.Add(new Fractal(x, y, count));
                }
            }
            var data = new FractalData(maxX, maxY, maxCount, pos);
            return data;
        }
    }
    class FractalData
    {
        public FractalData(int width, int height, int maxDepth, List<Fractal> fractals)
        {
            Width = width;
            Height = height;
            MaxDepth = maxDepth;
            Fractals = fractals;
        }

        public int Width { get; }
        public int Height { get; }
        public int MaxDepth { get; }
        public List<Fractal> Fractals { get; }
    }

    class FractalSaver
    {
        Random ran;
        int r, g, b;

        public FractalSaver()
        {
            ran = new Random();
            r = ran.Next(0, 100);
            g = ran.Next(0, 100);
            b = ran.Next(0, 100);
        }

        public void SaveFractal(FractalData data, string filename)
        {
            Color color;
            var img = new Bitmap(data.Width, data.Height);
            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x++)
                {
                    img.SetPixel(x, y, Color.Purple);
                }
            }
            int colorType = 1;
            foreach (var fractal in data.Fractals)
            {
                color = Color.Black;
                if (fractal.X < data.Width && fractal.Y < data.Height)
                {
                    var fc = (int)fractal.Count;
                    if (colorType == 0)
                    {
                        color = Color.FromArgb(255, Math.Min(r * fc, 255), Math.Min(g * fc, 255), Math.Min(b * fc, 255));
                    }
                    else if (colorType == 1)
                    {
                        double quotient = (double)fractal.Count / (double)data.MaxDepth;
                        double colorVal = Math.Clamp(quotient, 0f, 1.0f);
                        var rgb = new byte[3];
                        if (quotient > 0.5)
                        {
                            // Close to the mandelbrot set the color changes from green to white
                            rgb[0] = (byte)(colorVal * 255);
                            rgb[1] = 255;
                            rgb[2] = (byte)(colorVal * 255);
                        }
                        else
                        {
                            // Far away it changes from black to green
                            rgb[0] = 0;
                            rgb[1] = (byte)(colorVal * 255);
                            rgb[2] = 0;
                        }
                        color = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
                    }
                    if (fc >= data.MaxDepth) color = Color.Black;

                    img.SetPixel(fractal.X, fractal.Y, color);
                }
            }
            img.Save(filename, ImageFormat.Png);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var width = 2000;
            var height = 2000;
            var depth = 200;

            var maker = new FractalMaker();

            var data = maker.GenerateFractals(-1, -1, 1, 1, width, height, depth);

            var saver = new FractalSaver();
            saver.SaveFractal(data, "Test.png");
        }
    }
}
