using DotImaging;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace LibImageUtilities;
public static class ImageUtilities_BMP
{
    public static bool IsBmp(byte[] img) =>
        img.Length >= 2 &&
        img[0] == 0x42 &&
        img[1] == 0x4D &&
        img.Length >= 54;

    /// <summary>
    /// Get BMP image from PNG or BMP image.
    /// </summary>
    /// <param name="img"></param>
    /// <returns></returns>
    public static byte[] GetBmp(byte[] img) => IsBmp(img) ? img : ConvertPngToBmp(img);
    /// <summary>
    /// Get BMP image from PNG or BMP image. Sets the DPI in the BMP image, if needed.
    /// </summary>
    /// <param name="img"></param>
    /// <param name="dpiX"></param>
    /// <param name="dpiY"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static byte[] GetBmp(byte[] img, int dpiX, int dpiY = 0)
    {
        if (dpiX <= 0)
        {
            throw new ArgumentException("DPI value must be greater than zero.");
        }

        if (dpiY <= 0)
        {
            dpiY = dpiX; // Use dpiX if dpiY is not provided or invalid
        }

        if (IsBmp(img))
        {
            ImageUtilities.DPI dpi = GetBmpDPI(img);
            if (dpi.X == dpiX && dpi.Y == dpiY)
            {
                return img;
            }
            else
            {
                return SetBitmapDPI(img, dpiX, dpiY);
            }
        }

        return ConvertPngToBmp(img);
    }
    public static byte[] ConvertPngToBmp(byte[] png)
    {
        using MemoryStream ms = new(png);
        using var bitmap = new Bitmap(ms);

        // Determine the appropriate type for the ToImage<TColor> method
        var pixelFormat = ImageUtilities_PNG.GetPngPixelFormat(png);

        switch (pixelFormat)
        {
            case PixelFormat.Format24bppRgb:
                return bitmap.ToImage<Bgr<byte>>().Encode(".bmp");
            case PixelFormat.Format32bppArgb:
                return bitmap.ToImage<Bgra<byte>>().Encode(".bmp");
            case PixelFormat.Format8bppIndexed:
                return bitmap.ToImage<Gray<byte>>().Encode(".bmp");
            default:
                throw new NotSupportedException($"Pixel format {pixelFormat} is not supported.");
        }
    }

    public static ImageUtilities.DPI GetBmpDPI(byte[] image) =>
        IsBmp(image) == false
            ? throw new ArgumentException("The provided byte array is not a valid BMP image.")
            : new ImageUtilities.DPI
            {
                X = ImageUtilities.DotsPerInch(BitConverter.ToInt32(image, 38)),
                Y = ImageUtilities.DotsPerInch(BitConverter.ToInt32(image, 42))
            };
    public static int GetBmpWidth(byte[] image) =>
        IsBmp(image) == false
            ? throw new ArgumentException("The provided byte array is not a valid BMP image.")
            : BitConverter.ToInt32(image, 18);
    public static int GetBmpHeight(byte[] image) =>
    IsBmp(image) == false
        ? throw new ArgumentException("The provided byte array is not a valid BMP image.")
        : BitConverter.ToInt32(image, 22);
    /// <summary>
    /// Get the pixel format of a BMP image by reading the header bytes.
    /// </summary>
    /// <param name="image">BMP image byte array</param>
    /// <returns>PixelFormat</returns>
    public static PixelFormat GetBmpPixelFormat(byte[] image)
    {
        if (!IsBmp(image))
            throw new ArgumentException("The provided byte array is not a valid BMP image.");

        // Bit count per pixel is at byte 28 in the BMP header
        int bitCount = BitConverter.ToInt16(image, 28);

        return bitCount switch
        {
            1 => PixelFormat.Format1bppIndexed,
            4 => PixelFormat.Format4bppIndexed,
            8 => PixelFormat.Format8bppIndexed,
            16 => PixelFormat.Format16bppRgb565,
            24 => PixelFormat.Format24bppRgb,
            32 => PixelFormat.Format32bppArgb,
            _ => throw new NotSupportedException("Unsupported BMP bit count.")
        };
    }
    public static byte[] SetBitmapDPI(byte[] image, int dpiX, int dpiY)
    {
        if (!IsBmp(image))
            throw new ArgumentException("The provided byte array is not a valid BMP image.");

        int dpiXInMeters = ImageUtilities.DotsPerMeter(dpiX);
        int dpiYInMeters = ImageUtilities.DotsPerMeter(dpiY);

        // Set the horizontal DPI
        for (int i = 38; i < 42; i++)
        {
            image[i] = BitConverter.GetBytes(dpiXInMeters)[i - 38];
        }

        // Set the vertical DPI
        for (int i = 42; i < 46; i++)
        {
            image[i] = BitConverter.GetBytes(dpiYInMeters)[i - 42];
        }

        return image;
    }
    /// <summary>
    /// Converts a BMP byte array to a new PixelFormat.
    /// </summary>
    /// <param name="image">BMP image byte array</param>
    /// <param name="newPixelFormat">The new PixelFormat</param>
    /// <returns>Converted BMP image byte array</returns>
    public static byte[] SetBmpPixelFormat(byte[] image, PixelFormat newPixelFormat)
    {
        if (!IsBmp(image))
            throw new ArgumentException("The provided byte array is not a valid BMP image.");

        using MemoryStream inputStream = new(image);
        using Bitmap originalBitmap = new(inputStream);
        using Bitmap newBitmap = originalBitmap.Clone(new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height), newPixelFormat);

        using MemoryStream outputStream = new();
        newBitmap.Save(outputStream, ImageFormat.Bmp);
        return outputStream.ToArray();
    }
    public static byte[] ExtractBitmapData(byte[] image)
    {
        if (!IsBmp(image))
            throw new ArgumentException("The provided byte array is not a valid BMP image.");

        int dataOffset = BitConverter.ToInt32(image, 10);
        return image[dataOffset..];
    }
    public static byte[] ExtractBitmapIndexedColorPallet(byte[] image)
    {
        if (!IsBmp(image))
            throw new ArgumentException("The provided byte array is not a valid BMP image.");

        int dataOffset = BitConverter.ToInt32(image, 10);
        int palletOffset = 54;

        return image[palletOffset..dataOffset];
    }
}
