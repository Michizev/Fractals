namespace Fractals
{
    interface IFractalMaker
    {
        FractalData GenerateFractals(float left, float top, float xside, float yside, int maxX, int maxY, int maxCount);
    }
}