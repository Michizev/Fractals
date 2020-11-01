using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
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
    class RandomColorMaker : IColorMaker
    {
        Random ran;
        int r, g, b;
        public RandomColorMaker()
        {
            ran = new Random();
            r = ran.Next(0, 100);
            g = ran.Next(0, 100);
            b = ran.Next(0, 100);
        }

        public Color GetColor(FractalData data, Fractal fractal)
        {
            var fc = (int)fractal.Count;
            return Color.FromArgb(255, Math.Min(r * fc, 255), Math.Min(g * fc, 255), Math.Min(b * fc, 255));
        }

        
    }
    class GreenColorMaker : IColorMaker
    {
        public Color GetColor(FractalData data, Fractal fractal)
        {
            Color color;
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
            return color;
        }
    }

    class FractalSaver
    {
        public void SaveFractal(FractalData data, IColorMaker colorMaker, string filename)
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

            foreach (var fractal in data.Fractals)
            {
                color = Color.Black;
                if (fractal.X < data.Width && fractal.Y < data.Height)
                {
                    if (fractal.Count >= data.MaxDepth)
                    {
                        color = Color.Black;
                    }
                    else
                    {
                        color = colorMaker.GetColor(data, fractal);
                    }

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

            IFractalMaker maker = null;

            IEnumerable<Type> getImplementation(Type @interface) => AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(y => @interface.IsAssignableFrom(y) && !y.IsInterface);

            var fractals = getImplementation(typeof(IFractalMaker));
            var colors = getImplementation(typeof(IColorMaker));

            int selection = -1;
            int colorSelection = -1;
            List<Type> algorithms = GetImplementations(fractals);
            List<Type> colorAlgorithms = GetImplementations(colors);

            while (true)
            {
                selection = -1;
                var input = UserInput(algorithms, colorAlgorithms);
                if (input != null)
                {
                    var val = input.Value;
                    width = val.width;
                    height = val.height;
                    depth = val.depth;

                    selection = val.selection;
                    colorSelection = val.colorAlgorithm;
                    break;
                }
            }


            maker = (IFractalMaker)Activator.CreateInstance(algorithms[selection]);



            var data = maker.GenerateFractals(-1, -1, 1, 1, width, height, depth);

            var saver = new FractalSaver();

            var color = (IColorMaker)Activator.CreateInstance(colorAlgorithms[colorSelection]);

            Console.WriteLine("####SAVING FILES####");

            saver.SaveFractal(data, color, "Test.png");
        }

        private static List<Type> GetImplementations(IEnumerable<Type> fractals)
        {
            List<Type> algorithms = new List<Type>();
            foreach (var i in fractals)
            {
                algorithms.Add(i);
            }

            return algorithms;
        }

        private static (int width, int height, int depth, int selection, int colorAlgorithm)? UserInput(List<Type> algorithms, List<Type> colorAlgorithms)
        {
            int width = 0,
                height =0,
                depth=0;


            Console.WriteLine("Please pick an algorithm");
            int j = 0;
            foreach (var i in algorithms)
            {
                Console.WriteLine($"{j++} {i.Name}");
            }

            try
            {
                var userInput = Console.ReadLine();
                int selection = int.Parse(userInput);

                Console.WriteLine("Please pick a size (XxY) and the how many iterations should be run");
                userInput = Console.ReadLine();
                var splits = userInput.Split(" ");

                if (splits.Length == 3)
                {
                    width = int.Parse(splits[0]);
                    height = int.Parse(splits[1]);
                    depth = int.Parse(splits[2]);
                }

                Console.WriteLine("Please pick an color");
                j = 0;
                foreach (var i in colorAlgorithms)
                {
                    Console.WriteLine($"{j++} {i.Name}");
                }
                userInput = Console.ReadLine();
                int colorAlgorithm = int.Parse(userInput); 

                return (width, height, depth, selection, colorAlgorithm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Ups something went wrong, want to try again? (Y/n)");
                int ask = 0;
                do
                {
                    ask = AskForRepeat();
                } while (ask == -1);
                if (ask == 1)
                {
                    return null;
                }
                Environment.Exit(1);
                return null;
            }
        }

        private static int AskForRepeat()
        {
            var read = Console.ReadLine();
            if (read.Length == 0)
            {
                return 1;
            }
            return (read.ToLowerInvariant()) switch
            {
                "n" => 0,
                "y" => 1,
                _ => -1,
            };
        }
    }
}
