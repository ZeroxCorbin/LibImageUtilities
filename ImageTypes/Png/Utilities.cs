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
            var dpi = GetDPI(img);
            if (dpi.X == dpiX && dpi.Y == dpiY)
            {
                return img;
            }
            else
            {
                return SetDPI(img, dpiX, dpiY); ;
            }
        }

        return ConvertBmpToPng(img);
    }
    public static byte[] GetPng(byte[] img, ImageUtilities.DPI dpi) => GetPng(img, dpi.X, dpi.Y);
    public static byte[] GetPng(byte[] img, PixelFormat pixelFormat) => IsPng(img) ? SetPixelFormat(img, pixelFormat) : ConvertBmpToPng(img, pixelFormat);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public static byte[] ConvertBmpToPng(byte[] bmp, PixelFormat? pixelFormat = null)
    {
        // Get the DPI from the BMP image
        var dpi = Bmp.Utilities.GetDPI(bmp);
        // Determine the appropriate type for the ToImage<TColor> method
        pixelFormat = pixelFormat ?? Bmp.Utilities.GetPixelFormat(bmp);

        using MemoryStream ms = new(bmp);
        using var bitmap = new Bitmap(ms);
        byte[] png = pixelFormat switch
        {
            PixelFormat.Format24bppRgb => bitmap.ToImage<Bgr<byte>>().Encode(".png"),
            PixelFormat.Format32bppArgb => bitmap.ToImage<Bgra<byte>>().Encode(".png"),
            PixelFormat.Format8bppIndexed => bitmap.ToImage<Gray<byte>>().Encode(".png"),
            _ => throw new NotSupportedException($"Pixel format {pixelFormat} is not supported."),
        };
        return SetDPI(png, dpi);
    }

    public static int GetWidth(byte[] image)
    {
        if (!IsPng(image))
            throw new ArgumentException("The provided byte array is not a valid PNG image.");

        var png = new Png(image);
        if (png.Chunks.TryGetValue(ChunkTypes.IHDR, out IChunk? value))
        {
            IHDR_Chunk ihdrChunk = (IHDR_Chunk)value;
            return ihdrChunk.Width;
        }
        else
        {
            throw new ArgumentException("The PNG image does not contain an IHDR chunk.");
        }
    }
    public static int GetHeight(byte[] image)
    {
        if (!IsPng(image))
            throw new ArgumentException("The provided byte array is not a valid PNG image.");

        var png = new Png(image);
        if (png.Chunks.TryGetValue(ChunkTypes.IHDR, out IChunk? value))
        {
            IHDR_Chunk ihdrChunk = (IHDR_Chunk)value;
            return ihdrChunk.Height;
        }
        else
        {
            throw new ArgumentException("The PNG image does not contain an IHDR chunk.");
        }
    }

    public static ImageUtilities.DPI GetDPI(byte[] image)
    {
        if (!IsPng(image))
            throw new ArgumentException("The provided byte array is not a valid PNG image.");

        var png = new Png(image);
        if (png.Chunks.TryGetValue(ChunkTypes.pHYs, out IChunk? value))
        {
            PHYS_Chunk physChunk = (PHYS_Chunk)value;
            return new ImageUtilities.DPI
            {
                X = physChunk.DpiX,
                Y = physChunk.DpiY
            };
        }

        return null;
    }
    public static byte[] SetDPI(byte[] image, ImageUtilities.DPI dpi) => SetDPI(image, dpi.X, dpi.Y);
    public static byte[] SetDPI(byte[] image, int dpiX, int dpiY)
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
    /// Get the pixel format of a PNG image by reading the header bytes.
    /// </summary>
    /// <param name="image">PNG image byte array</param>
    /// <returns>PixelFormat</returns>
    public static PixelFormat GetPixelFormat(byte[] image)
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
    /// <summary>
    /// Converts a PNG byte array to a new PixelFormat.
    /// </summary>
    /// <param name="image">PNG image byte array</param>
    /// <param name="newPixelFormat">The new PixelFormat</param>
    /// <returns>Converted PNG image byte array</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public static byte[] SetPixelFormat(byte[] image, PixelFormat pixelFormat)
    {
        if (!IsPng(image))
            throw new ArgumentException("The provided byte array is not a valid PNG image.");

        if (pixelFormat == GetPixelFormat(image))
            return image;

        // Get the DPI from the BMP image
        var dpi = GetDPI(image);

        using MemoryStream ms = new(image);
        using var bitmap = new Bitmap(ms);
        byte[] png = pixelFormat switch
        {
            PixelFormat.Format24bppRgb => bitmap.ToImage<Bgr<byte>>().Encode(".png"),
            PixelFormat.Format32bppArgb => bitmap.ToImage<Bgra<byte>>().Encode(".png"),
            PixelFormat.Format8bppIndexed => bitmap.ToImage<Gray<byte>>().Encode(".png"),
            _ => throw new NotSupportedException($"Pixel format {pixelFormat} is not supported."),
        };
        return dpi != null ? SetDPI(png, dpi) : png;
    }

    public static byte[] GetImageData(byte[] image)
    {
        if (!IsPng(image))
            throw new ArgumentException("The provided byte array is not a valid PNG image.");

        var chunks = GetChunks(image);
        return chunks.TryGetValue(ChunkTypes.IDAT, out IChunk value) ? [.. value.Data] : ([]);
    }

    public static Dictionary<ChunkTypes, IChunk> GetChunks(byte[] png)
    {
        Dictionary<ChunkTypes, IChunk> chunks = [];

        using MemoryStream ms = new(png);
        using BinaryReader reader = new(ms);

        // Skip the PNG signature
        reader.BaseStream.Seek(Signature.Length, SeekOrigin.Begin);

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            int chunkLength = BitConverter.ToInt32(reader.ReadBytes(Png.IntSize).Reverse().ToArray(), 0);
            int chunkType = BitConverter.ToInt32(reader.ReadBytes(Png.IntSize).Reverse().ToArray(), 0);

            if (Enum.IsDefined(typeof(ChunkTypes), chunkType))
            {
                if (!chunks.ContainsKey((ChunkTypes)chunkType))
                {
                    chunks.Add((ChunkTypes)chunkType, GetChunkData(reader, chunkLength, (ChunkTypes)chunkType));
                    if ((ChunkTypes)chunkType == ChunkTypes.IEND)
                        break;
                }

                else
                {
                    chunks[(ChunkTypes)chunkType].Data.AddRange(reader.ReadBytes(chunkLength));
                    reader.BaseStream.Seek(IChunk.CrcSize, SeekOrigin.Current);
                }
            }
            else
                reader.BaseStream.Seek(IChunk.CrcSize + chunkLength, SeekOrigin.Current);
        }

        return chunks;
    }
    private static IChunk GetChunkData(BinaryReader reader, int chunkLength, ChunkTypes chunkType)
    {
        List<byte> data = [];

        for (int i = 0; i < chunkLength + IChunk.CrcSize; i++)
            data.Add(reader.ReadByte());

        return chunkType switch
        {
            ChunkTypes.IHDR => new IHDR_Chunk(data),
            ChunkTypes.PLTE => new Generic_Chunk(data, chunkType),
            ChunkTypes.IDAT => new IDAT_Chunk(data, true),
            ChunkTypes.IEND => new Generic_Chunk(data, chunkType),
            ChunkTypes.pHYs => new PHYS_Chunk(data),
            ChunkTypes.tEXt => new Generic_Chunk(data, chunkType),
            ChunkTypes.zTXt => new Generic_Chunk(data, chunkType),
            ChunkTypes.tIME => new Generic_Chunk(data, chunkType),
            ChunkTypes.tRNS => new Generic_Chunk(data, chunkType),
            ChunkTypes.cHRM => new Generic_Chunk(data, chunkType),
            ChunkTypes.gAMA => new Generic_Chunk(data, chunkType),
            ChunkTypes.iCCP => new Generic_Chunk(data, chunkType),
            ChunkTypes.sBIT => new Generic_Chunk(data, chunkType),
            ChunkTypes.sRGB => new Generic_Chunk(data, chunkType),
            ChunkTypes.iTXt => new Generic_Chunk(data, chunkType),
            ChunkTypes.bKGD => new Generic_Chunk(data, chunkType),
            ChunkTypes.hIST => new Generic_Chunk(data, chunkType),
            ChunkTypes.pCAL => new Generic_Chunk(data, chunkType),
            ChunkTypes.sPLT => new Generic_Chunk(data, chunkType),
            _ => new Generic_Chunk(data, chunkType),
        };
    }
}
