using System.Drawing;

namespace Fractals
{
    interface IColorMaker
    {
        Color GetColor(FractalData data, Fractal fractal);
    }
}