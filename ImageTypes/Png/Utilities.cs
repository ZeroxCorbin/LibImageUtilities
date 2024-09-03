using DotImaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace LibImageUtilities.ImageTypes.Png;
public static class Utilities
{
    public static bool IsPng(byte[] img) =>
    img.Length >= 8 &&
    img[0] == 0x89 &&
    img[1] == 0x50 &&
    img[2] == 0x4E &&
    img[3] == 0x47 &&
    img[4] == 0x0D &&
    img[5] == 0x0A &&
    img[6] == 0x1A &&
    img[7] == 0x0A;

    /// <summary>
    /// Get PNG image from PNG or BMP image.
    /// Copies DPI, PixelFormat, and metadata if converted from BMP.
    /// </summary>
    /// <param name="img"></param>
    /// <returns>Converted BMP or original image</returns>
    public static byte[] GetPng(byte[] img) => IsPng(img) ? img : ConvertBmpToPng(img);
    /// <summary>
    /// Get PNG image from PNG or BMP image. Sets the DPI in the PNG image, if needed.
    /// Copies PixelFormat and metadata if converted from BMP.
    /// </summary>
    /// <param name="img"></param>
    /// <param name="dpiX"></param>
    /// <param name="dpiY"></param>
    /// <returns>Converted BMP or updated orginal</returns>
    /// <exception cref="ArgumentException"></exception>
    public static byte[] GetPng(byte[] img, int dpiX, int dpiY = 0)
    {
        if (dpiX <= 0)
        {
            throw new ArgumentException("DPI value must be greater than zero.");
        }

        if (dpiY <= 0)
        {
            dpiY = dpiX; // Use dpiX if dpiY is not provided or invalid
        }

        if (IsPng(img))
        {
            var dpi = GetPngDPI(img);
            if (dpi.X == dpiX && dpi.Y == dpiY)
            {
                return img;
            }
            else
            {
                return SetPngDPI(img, dpiX, dpiY); ;
            }
        }

        return ConvertBmpToPng(img);
    }
    public static byte[] GetPng(byte[] img, PixelFormat pixelFormat)
    {
        //if (IsPng(img))
        //    return GetPngPixelFormat(img) == pixelFormat ? img : SetPngPixelFormat(img, pixelFormat);

        //byte[] res = IsBmp(img) ? SetBmpPixelFormat(img, pixelFormat) : throw new ArgumentException("Unsupported image format.");

        return ConvertBmpToPng(img);
    }
    public static byte[] ConvertBmpToPng(byte[] bmp)
    {
        // Get the DPI from the BMP image
        var dpi = ImageUtilities_BMP.GetBmpDPI(bmp);
        // Determine the appropriate type for the ToImage<TColor> method
        var pixelFormat = ImageUtilities_BMP.GetBmpPixelFormat(bmp);

        using MemoryStream ms = new(bmp);
        using var bitmap = new Bitmap(ms);
        byte[] png;
        switch (pixelFormat)
        {
            case PixelFormat.Format24bppRgb:
                png = bitmap.ToImage<Bgr<byte>>().Encode(".png");
                break;
            case PixelFormat.Format32bppArgb:
                png = bitmap.ToImage<Bgra<byte>>().Encode(".png");
                break;
            case PixelFormat.Format8bppIndexed:
                png = bitmap.ToImage<Gray<byte>>().Encode(".png");
                break;
            default:
                throw new NotSupportedException($"Pixel format {pixelFormat} is not supported.");
        }

        var pngFormat = GetPngPixelFormat(png);
        string pngs = System.Text.Encoding.Default.GetString(png);
        return SetPngDPI(png, dpi);
    }

    public static ImageUtilities.DPI GetPngDPI(byte[] image)
    {
        if (!IsPng(image))
            throw new ArgumentException("The provided byte array is not a valid PNG image.");

        var chunks = Png.GetPngChunks(image);
        if (chunks.TryGetValue(ChunkTypes.pHYs, out IChunk value))
        {
            PHYS_Chunk physChunk = (PHYS_Chunk)value;
            return new ImageUtilities.DPI
            {
                X = physChunk.DpiX,
                Y = physChunk.DpiY
            };
        }

        throw new ArgumentException("pHYs chunk not found in PNG file.");
    }
    /// <summary>
    /// Get the pixel format of a PNG image by reading the header bytes.
    /// </summary>
    /// <param name="image">PNG image byte array</param>
    /// <returns>PixelFormat</returns>
    public static PixelFormat GetPngPixelFormat(byte[] image)
    {
        if (!IsPng(image))
            throw new ArgumentException("The provided byte array is not a valid PNG image.");

        // Color type is at byte 25 in the PNG header
        byte colorType = image[25];

        return colorType switch
        {
            0 => PixelFormat.Format8bppIndexed, // Grayscale
            2 => PixelFormat.Format24bppRgb,    // Truecolor
            3 => PixelFormat.Format8bppIndexed, // Indexed-color
            4 => PixelFormat.Format16bppGrayScale, // Grayscale with alpha
            6 => PixelFormat.Format32bppArgb,   // Truecolor with alpha
            _ => throw new NotSupportedException("Unsupported PNG color type.")
        };
    }
    public static byte[] SetPngDPI(byte[] image, ImageUtilities.DPI dpi) => SetPngDPI(image, dpi.X, dpi.Y);
    public static byte[] SetPngDPI(byte[] image, int dpiX, int dpiY)
    {
        if (!IsPng(image))
            throw new ArgumentException("The provided byte array is not a valid PNG image.");

        var png = new Png(image)
        {
            pHYs = new PHYS_Chunk(dpiX, dpiY)
        };
        return png.GetBytes();
    }

    /// <summary>
    /// Converts a PNG byte array to a new PixelFormat.
    /// </summary>
    /// <param name="image">PNG image byte array</param>
    /// <param name="newPixelFormat">The new PixelFormat</param>
    /// <returns>Converted PNG image byte array</returns>
    public static byte[] SetPngPixelFormat(byte[] image, PixelFormat newPixelFormat)
    {
        if (!IsPng(image))
            throw new ArgumentException("The provided byte array is not a valid PNG image.");

        using MemoryStream inputStream = new(image);
        using Bitmap originalBitmap = new(inputStream);
        Bitmap newBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height, newPixelFormat);

        // Use Graphics to draw the original bitmap onto the new bitmap
        using (Graphics g = Graphics.FromImage(newBitmap))
            g.DrawImage(originalBitmap, new Rectangle(0, 0, newBitmap.Width, newBitmap.Height));

        using MemoryStream outputStream = new();
        newBitmap.Save(outputStream, ImageFormat.Png);
        return outputStream.ToArray();
    }

    public static byte[] ExtractIDATData(byte[] image)
    {
        if (!IsPng(image))
            throw new ArgumentException("The provided byte array is not a valid PNG image.");

        var chunks = Png.GetPngChunks(image);
        return chunks.TryGetValue(ChunkTypes.IDAT, out IChunk value) ? [.. value.Data] : ([]);
    }
}
