namespace QiuSkiaBridge;
using SkiaSharp;
using System.IO;

public class SkiaHelper
{
    public static SKPngEncoderOptions GetPngEncoderOptions()
    {
        return new SKPngEncoderOptions(
            SKPngEncoderFilterFlags.AllFilters,
            /*zLibLevel=*/ 4
        );
    }
}