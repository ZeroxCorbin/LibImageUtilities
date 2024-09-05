using System;
using System.Drawing.Imaging;

namespace LibImageUtilities.ImageTypes.Bmp;
public static class ByteArrayExtensions
{
    public static bool IsBmp(this byte[] img) => Utilities.IsBmp(img);

    /// <summary>
    /// Get BMP image from BMP or PNG image.
    /// Copies DPI, PixelFormat, and metadata if converted from PNG.
    /// </summary>
    /// <param name="img"></param>
    /// <returns>Converted PNG or original image</returns>
    public static byte[] GetBmp(this byte[] img) => Utilities.GetBmp(img);
    /// <summary>
    /// Get BMP image from BMP or PNG image. Sets the DPI in the BMP image, if needed.
    /// Copies PixelFormat and metadata if converted from PNG.
    /// </summary>
    /// <param name="img"></param>
    /// <param name="dpiX"></param>
    /// <param name="dpiY"></param>
    /// <returns>Converted PNG or updated orginal</returns>
    /// <exception cref="ArgumentException"></exception>
    public static byte[] GetBmp(this byte[] img, int dpiX, int dpiY = 0) => Utilities.GetBmp(img, dpiX, dpiY);
    public static byte[] GetBmp(this byte[] img, ImageUtilities.DPI dpi) => Utilities.GetBmp(img, dpi);
    public static byte[] GetBmp(this byte[] img, PixelFormat pixelFormat) => Utilities.GetBmp(img, pixelFormat);

    public static byte[] ConvertPngToBmp(this byte[] png, PixelFormat? pixelFormat = null) => Utilities.ConvertPngToBmp(png, pixelFormat);
}
