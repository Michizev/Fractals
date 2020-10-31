using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

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
    class SIMDVectorFractalMaker : IFractalMaker
    {
        readonly int smidLength;
        public SIMDVectorFractalMaker()
        {
            smidLength = Vector<float>.Count;
        }
        public FractalData GenerateFractals(float left, float top, float xside, float yside, int maxX, int maxY, int maxCount)
        {
            float xscale, yscale;

            // setting up the xscale and yscale 
            xscale = (float)xside / maxX;
            yscale = (float)yside / maxY;

            var content = new float[smidLength];
            var locks = new bool[smidLength];
            var counts = new int[smidLength];

            List<Fractal> pos = new List<Fractal>
            {
                Capacity = maxX * maxY
            };

            for (int y = 0; y < maxY; y++)
            {
                content = new float[maxX];
                for (int i = 0; i < maxX; i++)
                {
                    content[i] = (i) * xscale + left;
                }

                for (int x = 0; x < maxX; x += smidLength)
                {
                    var leftoverLength = Math.Min(smidLength, maxX - x);

                    for (int i = 0; i < leftoverLength; i++)
                    {

                        counts[i] = 0;
                        locks[i] = true;
                    }

                    var cReal = new Vector<float>(content, x);

                    var cImag = new Vector<float>(y * yscale + top);
                    var zReal = Vector<float>.Zero;
                    var zImag = Vector<float>.Zero;

                    if (x == 0 && y == 0)
                    {
                        Console.WriteLine(cReal);
                    }

                    var edited = true;
                    var loops = 0;

                    var zRealSquare = zReal * zReal;
                    var zImagSquare = zImag * zImag;

                    while (edited && loops < maxCount)
                    {
                        edited = false;

                        //tempx = zx * zx - zy * zy + cx;
                        var tmp = zRealSquare - zImagSquare + cReal;

                        // 2*z_real*z_imaginary + c_imaginary 
                        //zy = 2 * zx * zy + cy;
                        zImag = zImag * zReal * 2 + cImag;
                        // Updating z_real = tempx 
                        //zx = tempx;
                        zReal = tmp;

                        zRealSquare = zReal * zReal;
                        zImagSquare = zImag * zImag;

                        //(zx * zx + zy * zy < 4)

                        var test = zRealSquare + zImagSquare;
                        for (int i = 0; i < leftoverLength; i++)
                        {
                            if (locks[i] && test[i] < 4)
                            {
                                edited = true;
                                counts[i] += 1;
                            }
                            else
                            {
                                locks[i] = false;
                            }
                        }
                        ++loops;
                    }

                    for (int i = 0; i < leftoverLength; i++)
                    {
                        if (counts[i] < maxCount)
                        {
                            counts[i] += 1;
                        }
                        pos.Add(new Fractal(x + i, y, counts[i]));
                    }
                }
            }
            var data = new FractalData(maxX, maxY, maxCount, pos);
            return data;
        }
    }
    class SIMDVector4FractalMaker : IFractalMaker
    {
        public FractalData GenerateFractals(float left, float top, float xside, float yside, int maxX, int maxY, int maxCount)
        {
            List<Fractal> pos = new List<Fractal>
            {
                Capacity = maxX * maxY
            };

            float xscale, yscale;

            // setting up the xscale and yscale 
            xscale = (float)xside / maxX;
            yscale = (float)yside / maxY;

            const int len = 4;

            var locks = new bool[len];
            var counts = new int[len];


            for (int y = 0; y < maxY; y++)
            {
                for (int x = 0; x < maxX; x += 4)
                {
                    // c_real 
                    //cx = x * xscale + left;
                    var cReal = new Vector4(x * xscale + left, (x + 1) * xscale + left, (x + 2) * xscale + left, (x + 3) * xscale + left);
                    // c_imaginary 
                    //cy = y * yscale + top;

                    var cImag = new Vector4(y * yscale + top);
                    var zReal = Vector4.Zero;
                    var zImag = Vector4.Zero;

                    counts = new int[4];
                    locks = new bool[] { true, true, true, true };

                    var edited = true;
                    var loops = 0;

                    
                    var zRealSquare = zReal * zReal;
                    var zImagSquare = zImag * zImag;

                    while (edited && loops < maxCount)
                    {
                        edited = false;

                        //tempx = zx * zx - zy * zy + cx;
                        var tmp = zRealSquare - zImagSquare + cReal;

                        // 2*z_real*z_imaginary + c_imaginary 
                        //zy = 2 * zx * zy + cy;
                        zImag = zImag * zReal * 2 + cImag;
                        // Updating z_real = tempx 
                        //zx = tempx;
                        zReal = tmp;

                        zRealSquare = zReal * zReal;
                        zImagSquare = zImag * zImag;

                        //(zx * zx + zy * zy < 4)
                        var test = zRealSquare + zImagSquare;

                        if (locks[0] && test.X < 4)
                        {
                            edited = true;
                            counts[0] += 1;
                        }
                        else
                        {
                            locks[0] = false;
                        }

                        if (locks[1] && test.Y < 4)
                        {
                            edited = true;
                            counts[1] += 1;
                        }
                        else
                        {
                            locks[1] = false;
                        }

                        if (locks[2] && test.Z < 4)
                        {
                            edited = true;
                            counts[2] += 1;
                        }
                        else
                        {
                            locks[2] = false;
                        }

                        if (locks[3] && test.W < 4)
                        {
                            edited = true;
                            counts[3] += 1;
                        }
                        else
                        {
                            locks[3] = false;
                        }
                        ++loops;
                    }
                    for (int i = 0; i < counts.Length; i++)
                    {
                        if (counts[i] < maxCount)
                        {
                            counts[i]++;
                        }
                    }

                    //Add all four fractals to the List
                    for(int i=0;i<4;i++)
                    {
                        pos.Add(new Fractal(x+i, y, (int)counts[i]));
                    }
                }

            }
            var data = new FractalData(maxX, maxY, maxCount, pos);
            return data;
        }
    }
    class FractalMaker : IFractalMaker
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

            var maker = new SIMDVectorFractalMaker();

            var data = maker.GenerateFractals(-1, -1, 1, 1, width, height, depth);

            var saver = new FractalSaver();
            saver.SaveFractal(data, "Test.png");
        }
    }
}
