using DotImaging;
using ImageMagick;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace LibImageUtilities.ImageTypes.Bmp;
public static class Utilities
{
    public static bool IsBmp(byte[] img) =>
        img.Length >= 2 &&
        img[0] == 0x42 &&
        img[1] == 0x4D &&
        img.Length >= 54;

    public static byte[] GetBmp(string path) => GetBmp(File.ReadAllBytes(path));
    public static byte[] GetBmp(string path, int dpiX, int dpiY = 0) => GetBmp(File.ReadAllBytes(path), dpiX, dpiY);
    public static byte[] GetBmp(string path, ImageUtilities.DPI dpi) => GetBmp(File.ReadAllBytes(path), dpi);
    public static byte[] GetBmp(string path, PixelFormat pixelFormat) => GetBmp(File.ReadAllBytes(path), pixelFormat);

    public static byte[] GetBmp(Stream stream) { using MemoryStream ms = new(); stream.CopyTo(ms); return GetBmp(ms.ToArray()); }
    public static byte[] GetBmp(Stream stream, int dpiX, int dpiY = 0) { using MemoryStream ms = new(); stream.CopyTo(ms); return GetBmp(ms.ToArray(), dpiX, dpiY); }
    public static byte[] GetBmp(Stream stream, ImageUtilities.DPI dpi) { using MemoryStream ms = new(); stream.CopyTo(ms); return GetBmp(ms.ToArray(), dpi); }
    public static byte[] GetBmp(Stream stream, PixelFormat pixelFormat) { using MemoryStream ms = new(); stream.CopyTo(ms); return GetBmp(ms.ToArray(), pixelFormat); }

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
            ImageUtilities.DPI dpi = GetDPI(img);
            if (dpi.X == dpiX && dpi.Y == dpiY)
            {
                return img;
            }
            else
            {
                return SetDPI(img, dpiX, dpiY);
            }
        }

        return ConvertPngToBmp(img, ignoreDPI: false);
    }
    public static byte[] GetBmp(byte[] img, ImageUtilities.DPI dpi) => GetBmp(img, dpi.X, dpi.Y);
    public static byte[] GetBmp(byte[] img, PixelFormat pixelFormat)
    {
        if (IsBmp(img))
            return SetPixelFormat(img, pixelFormat);
        else
            return ConvertPngToBmp(img, pixelFormat);
    }

    //[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    //public static byte[] ConvertPngToBmp(byte[] png, PixelFormat? pixelFormat = null, bool ignoreDPI = true)
    //{
    //    // Determine the appropriate type for the ToImage<TColor> method
    //    pixelFormat = pixelFormat ?? Png.Utilities.GetPixelFormat(png);

    //    using MemoryStream ms = new(png);
    //    using var bitmap = new Bitmap(ms);
    //    byte[] bmp = pixelFormat switch
    //    {
    //        PixelFormat.Format24bppRgb => bitmap.ToImage<Bgr<byte>>().Encode(".bmp"),
    //        PixelFormat.Format32bppArgb => bitmap.ToImage<Bgra<byte>>().Encode(".bmp"),
    //        PixelFormat.Format8bppIndexed => bitmap.ToImage<Gray<byte>>().Encode(".bmp"),
    //        _ => throw new NotSupportedException($"Pixel format {pixelFormat} is not supported."),
    //    };

    //    return ignoreDPI ? bmp : SetDPI(bmp, Png.Utilities.GetDPI(png));
    //}
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public static byte[] ConvertPngToBmp(byte[] png, PixelFormat? pixelFormat = null, bool ignoreDPI = true)
    {
        using MemoryStream ms = new(png);
        using var image = new MagickImage(ms);

        if (pixelFormat.HasValue)
        {
            image.Format = MagickFormat.Bmp3;
            switch (pixelFormat.Value)
            {
                case PixelFormat.Format24bppRgb:
                    image.Depth = 24;
                    break;
                case PixelFormat.Format32bppArgb:
                    image.Depth = 32;
                    break;
                case PixelFormat.Format8bppIndexed:
                    image.Depth = 8;
                    break;
                default:
                    throw new NotSupportedException($"Pixel format {pixelFormat} is not supported.");
            }
        }
        else
        {
            image.Format = MagickFormat.Bmp3;
        }

        if (!ignoreDPI)
        {
            var dpi = Png.Utilities.GetDPI(png);
            image.Density = new Density(dpi.X, dpi.Y, DensityUnit.PixelsPerInch);
        }

        using MemoryStream bmpStream = new();
        image.Write(bmpStream);
        return bmpStream.ToArray();
    }


    public static int GetWidth(byte[] image) =>
    IsBmp(image) == false
        ? throw new ArgumentException("The provided byte array is not a valid BMP image.")
        : BitConverter.ToInt32(image, 18);
    public static int GetHeight(byte[] image) =>
    IsBmp(image) == false
        ? throw new ArgumentException("The provided byte array is not a valid BMP image.")
        : BitConverter.ToInt32(image, 22);

    public static ImageUtilities.DPI GetDPI(byte[] image) =>
        IsBmp(image) == false
            ? throw new ArgumentException("The provided byte array is not a valid BMP image.")
            : new ImageUtilities.DPI
            {
                X = ImageUtilities.Dpm2Dpi(BitConverter.ToInt32(image, 38)),
                Y = ImageUtilities.Dpm2Dpi(BitConverter.ToInt32(image, 42))
            };
    public static byte[] SetDPI(byte[] image, ImageUtilities.DPI dpi) => SetDPI(image, dpi.X, dpi.Y);
    public static byte[] SetDPI(byte[] image, int dpiX, int dpiY)
    {
        if (!IsBmp(image))
            throw new ArgumentException("The provided byte array is not a valid BMP image.");

        int dpiXInMeters = ImageUtilities.Dpi2Dpm(dpiX);
        int dpiYInMeters = ImageUtilities.Dpi2Dpm(dpiY);

        // Set the horizontal DPI
        for (int i = 38; i < 42; i++)
            image[i] = BitConverter.GetBytes(dpiXInMeters)[i - 38];
        // Set the vertical DPI
        for (int i = 42; i < 46; i++)
            image[i] = BitConverter.GetBytes(dpiYInMeters)[i - 42];
        return image;
    }

    /// <summary>
    /// Get the pixel format of a BMP image by reading the header bytes.
    /// </summary>
    /// <param name="image">BMP image byte array</param>
    /// <returns>PixelFormat</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public static PixelFormat GetPixelFormat(byte[] image)
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
    /// <summary>
    /// Converts a BMP byte array to a new PixelFormat.
    /// </summary>
    /// <param name="image">BMP image byte array</param>
    /// <param name="newPixelFormat">The new PixelFormat</param>
    /// <returns>Converted BMP image byte array</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public static byte[] SetPixelFormat(byte[] image, PixelFormat pixelFormat)
    {
        if (!IsBmp(image))
            throw new ArgumentException("The provided byte array is not a valid BMP image.");

        if (pixelFormat == GetPixelFormat(image))
            return image;

        // Get the DPI from the BMP image
        var dpi = GetDPI(image);

        using MemoryStream ms = new(image);
        using var bitmap = new Bitmap(ms);
        byte[] png = pixelFormat switch
        {
            PixelFormat.Format24bppRgb => bitmap.ToImage<Bgr<byte>>().Encode(".bmp"),
            PixelFormat.Format32bppArgb => bitmap.ToImage<Bgra<byte>>().Encode(".bmp"),
            PixelFormat.Format8bppIndexed => bitmap.ToImage<Gray<byte>>().Encode(".bmp"),
            PixelFormat.Indexed => throw new NotImplementedException(),
            PixelFormat.Gdi => throw new NotImplementedException(),
            PixelFormat.Alpha => throw new NotImplementedException(),
            PixelFormat.PAlpha => throw new NotImplementedException(),
            PixelFormat.Extended => throw new NotImplementedException(),
            PixelFormat.Canonical => throw new NotImplementedException(),
            PixelFormat.Undefined => throw new NotImplementedException(),
            PixelFormat.Format1bppIndexed => throw new NotImplementedException(),
            PixelFormat.Format4bppIndexed => throw new NotImplementedException(),
            PixelFormat.Format16bppGrayScale => throw new NotImplementedException(),
            PixelFormat.Format16bppRgb555 => throw new NotImplementedException(),
            PixelFormat.Format16bppRgb565 => throw new NotImplementedException(),
            PixelFormat.Format16bppArgb1555 => throw new NotImplementedException(),
            PixelFormat.Format32bppRgb => throw new NotImplementedException(),
            PixelFormat.Format32bppPArgb => throw new NotImplementedException(),
            PixelFormat.Format48bppRgb => throw new NotImplementedException(),
            PixelFormat.Format64bppArgb => throw new NotImplementedException(),
            PixelFormat.Format64bppPArgb => throw new NotImplementedException(),
            PixelFormat.Max => throw new NotImplementedException(),
            _ => throw new NotSupportedException($"Pixel format {pixelFormat} is not supported."),
        };
        var pngFormat = GetPixelFormat(png);
        return SetDPI(png, dpi);
    }

    public static byte[] GetImageData(byte[] image)
    {
        if (!IsBmp(image))
            throw new ArgumentException("The provided byte array is not a valid BMP image.");

        int dataOffset = BitConverter.ToInt32(image, 10);
        return image[dataOffset..];
    }



    public static byte[] GetIndexedColorPallet(byte[] image)
    {
        if (!IsBmp(image))
            throw new ArgumentException("The provided byte array is not a valid BMP image.");

        int dataOffset = BitConverter.ToInt32(image, 10);
        int palletOffset = 54;

        return image[palletOffset..dataOffset];
    }
}
