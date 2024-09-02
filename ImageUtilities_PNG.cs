using DotImaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace LibImageUtilities;
public static class ImageUtilities_PNG
{
    public static class CRC32
    {
        /* Table of CRCs of all 8-bit messages. */
        private static readonly uint[] crcTable = new uint[256];

        /* Flag: has the table been computed? Initially false. */
        private static bool crcTableComputed = false;

        /* Make the table for a fast CRC. */
        private static void MakeCrcTable()
        {
            uint c;
            for (int n = 0; n < 256; n++)
            {
                c = (uint)n;
                for (int k = 0; k < 8; k++)
                {
                    if ((c & 1) != 0)
                        c = 0xedb88320U ^ (c >> 1);
                    else
                        c >>= 1;
                }
                crcTable[n] = c;
            }
            crcTableComputed = true;
        }

        /* Update a running CRC with the bytes buf[0..len-1]--the CRC
           should be initialized to all 1's, and the transmitted value
           is the 1's complement of the final running CRC (see the
           crc() routine below)). */
        private static uint UpdateCrc(uint crc, byte[] buf, int len)
        {
            uint c = crc;
            if (!crcTableComputed)
                MakeCrcTable();
            for (int n = 0; n < len; n++)
            {
                c = crcTable[(c ^ buf[n]) & 0xff] ^ (c >> 8);
            }
            return c;
        }

        /* Return the CRC of the bytes buf[0..len-1]. */
        public static uint ComputeCrc(byte[] buf, int len)
        {
            return UpdateCrc(0xffffffffU, buf, len) ^ 0xffffffffU;
        }
    }

    public enum Chunks
    {
        IHDR = 0x49484452,
        PLTE = 0x504C5445,
        IDAT = 0x49444154,
        IEND = 0x49454E44,
        pHYs = 0x70485973,
        tEXt = 0x74455874,
        zTXt = 0x7A545874,
        tIME = 0x74494D45,
        tRNS = 0x74524E53,
        cHRM = 0x6348524D,
        gAMA = 0x67414D41,
        iCCP = 0x69434350,
        sBIT = 0x73424954,
        sRGB = 0x73524742,
        iTXt = 0x69545874,
        bKGD = 0x624B4744,
        hIST = 0x68495354,
        pCAL = 0x7043414C,
        sPLT = 0x73504C54,
    }

    public class ChunkParameters
    {
        public Chunks Type { get; set; }
        public bool Critical { get; set; }
        public bool MultipleAllowed { get; set; }
        public int Length { get; set; }
    }

    public static ChunkParameters GetChunkParameters(Chunks chunk) => chunk switch
    {
        Chunks.IHDR => new ChunkParameters() { Type = Chunks.IHDR, Critical = true, MultipleAllowed = false, Length = 13 },
        Chunks.PLTE => new ChunkParameters() { Type = Chunks.PLTE, Critical = false, MultipleAllowed = false, Length = 0 },
        Chunks.IDAT => new ChunkParameters() { Type = Chunks.IDAT, Critical = true, MultipleAllowed = true, Length = 0 },
        Chunks.IEND => new ChunkParameters() { Type = Chunks.IEND, Critical = true, MultipleAllowed = false, Length = 0 },
        Chunks.pHYs => new ChunkParameters() { Type = Chunks.pHYs, Critical = false, MultipleAllowed = false, Length = 9 },
        Chunks.tEXt => new ChunkParameters() { Type = Chunks.tEXt, Critical = false, MultipleAllowed = true, Length = 0 },
        Chunks.zTXt => new ChunkParameters() { Type = Chunks.zTXt, Critical = false, MultipleAllowed = true, Length = 0 },
        Chunks.tIME => new ChunkParameters() { Type = Chunks.tIME, Critical = false, MultipleAllowed = false, Length = 7 },
        Chunks.tRNS => new ChunkParameters() { Type = Chunks.tRNS, Critical = false, MultipleAllowed = false, Length = 0 },
        Chunks.cHRM => new ChunkParameters() { Type = Chunks.cHRM, Critical = false, MultipleAllowed = false, Length = 32 },
        Chunks.gAMA => new ChunkParameters() { Type = Chunks.gAMA, Critical = false, MultipleAllowed = false, Length = 4 },
        Chunks.iCCP => new ChunkParameters() { Type = Chunks.iCCP, Critical = false, MultipleAllowed = false, Length = 0 },
        Chunks.sBIT => new ChunkParameters() { Type = Chunks.sBIT, Critical = false, MultipleAllowed = false, Length = 0 },
        Chunks.sRGB => new ChunkParameters() { Type = Chunks.sRGB, Critical = false, MultipleAllowed = false, Length = 1 },
        Chunks.iTXt => new ChunkParameters() { Type = Chunks.iTXt, Critical = false, MultipleAllowed = true, Length = 0 },
        Chunks.bKGD => new ChunkParameters() { Type = Chunks.bKGD, Critical = false, MultipleAllowed = false, Length = 0 },
        Chunks.hIST => new ChunkParameters() { Type = Chunks.hIST, Critical = false, MultipleAllowed = false, Length = 0 },
        Chunks.pCAL => new ChunkParameters() { Type = Chunks.pCAL, Critical = false, MultipleAllowed = false, Length = 0 },
        Chunks.sPLT => new ChunkParameters() { Type = Chunks.sPLT, Critical = false, MultipleAllowed = false, Length = 0 },
        _ => throw new ArgumentException("Unsupported chunk type.")
    };

    public class PNG_Signature : List<byte>
    {
        public const int Length = 8;
        public bool IsValid => this.Count >= 8 &&
            this[0] == 0x89 && //Non-ASCII unique ID
                               //The first byte is chosen as a non-ASCII value to reduce the probability that a text file may be misrecognized as a PNG file; also, it catches bad file transfers that clear bit 7
            this[1] == 0x50 && //ASCII 'P' considered part of the unique ID
            this[2] == 0x4E && //Bytes two [1] through four [3] name the format. ASCII 'P' 'N' 'G'
            this[3] == 0x47 &&
            this[4] == 0x0D && //The CR-LF sequence catches bad file transfers that alter newline sequences.
            this[5] == 0x0A &&
            this[6] == 0x1A && //The control-Z character stops file display under MS-DOS.
            this[7] == 0x0A;   //The final line feed checks for the inverse of the CR-LF translation problem.
        public int UniqueID => BitConverter.ToInt16(this.Take(2).ToArray(), 0);
        public string FormatName => System.Text.Encoding.Default.GetString(this.Skip(2).Take(3).ToArray());
        public bool ValidCrLf => this[4] == 0x0D && this[5] == 0x0A;
        public bool ValidControlZ => this[6] == 0x1A;
        public bool ValidFinalLf => this[7] == 0x0A;
    }

    public interface IChunk
    {
        int Length { get; }
        Chunks Type { get; }
        ChunkParameters Parameters { get; }
        List<byte> Data { get; }
        int CRC { get; }
        bool CheckCRC();
    }
    public class Generic_Chunk : List<byte>, IChunk
    {
        public Chunks Type { get; }
        public ChunkParameters Parameters => GetChunkParameters(Type);

        public int Length => this.Count;
        public List<byte> Data => this;

        public int CRC => BitConverter.ToInt32(this.Skip(this.Count - 4).Reverse().ToArray(), 0);
        public bool CheckCRC() => CRC == CRC32.ComputeCrc(this.Data.ToArray(), this.Data.Count);

        public Generic_Chunk(List<byte> chunk, Chunks type) : base(chunk) { Type = type; }
    }
    public class IHDR_Chunk : List<byte>, IChunk
    {
        public Chunks Type => Chunks.IHDR;
        public ChunkParameters Parameters => GetChunkParameters(Type);

        public int Length => this.Count;
        public List<byte> Data => this;

        public int CRC => BitConverter.ToInt32(this.Skip(this.Count - 4).Reverse().ToArray(), 0);
        public bool CheckCRC() => CRC == CRC32.ComputeCrc(this.Data.ToArray(), this.Data.Count);

        public int Width => BitConverter.ToInt32(this.Take(4).Reverse().ToArray(), 0);
        public int Height => BitConverter.ToInt32(this.Skip(4).Take(4).Reverse().ToArray(), 0);
        public byte BitDepth => this[8];
        public byte ColorType => this[9];
        public byte CompressionMethod => this[10];
        public byte FilterMethod => this[11];
        public byte InterlaceMethod => this[12];

        public IHDR_Chunk(List<byte> chunk) : base(chunk) { }
    }
    public class PHYS_Chunk : List<byte>, IChunk
    {
        public Chunks Type => Chunks.pHYs;
        public ChunkParameters Parameters => GetChunkParameters(Type);

        public int Length => this.Count;
        public List<byte> Data => this;

        public int CRC => BitConverter.ToInt32(this.Skip(this.Count - 4).Reverse().ToArray(), 0);
        public bool CheckCRC() => CRC == CRC32.ComputeCrc(this.Data.ToArray(), this.Data.Count);

        public int PixelsPerUnitX => BitConverter.ToInt32(this.Take(4).Reverse().ToArray(), 0);
        public int PixelsPerUnitY => BitConverter.ToInt32(this.Skip(4).Take(4).Reverse().ToArray(), 0);
        public byte UnitSpecifier => this[8];

        public int DpiX => UnitSpecifier == 0x01 ? (int)(PixelsPerUnitX * ImageUtilities.InchesPerMeter) : 0;
        public int DpiY => UnitSpecifier == 0x01 ? (int)(PixelsPerUnitY * ImageUtilities.InchesPerMeter) : 0;

        public PHYS_Chunk(List<byte> chunk) : base(chunk) { }
        public PHYS_Chunk(int pixelsPerUnitX, int pixelsPerUnitY, byte unitSpecifier)
        {
            AddRange(BitConverter.GetBytes(pixelsPerUnitX).Reverse());
            AddRange(BitConverter.GetBytes(pixelsPerUnitY).Reverse());
            Add(unitSpecifier);
        }
    }

    public static Dictionary<Chunks, IChunk> GetPngChunks(byte[] png)
    {
        Dictionary<Chunks, IChunk> chunks = new();

        using MemoryStream ms = new(png);
        using BinaryReader reader = new(ms);

        // Skip the PNG signature
        reader.BaseStream.Seek(8, SeekOrigin.Begin);

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            int chunkLength = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
            int chunkType = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);

            if (Enum.IsDefined(typeof(Chunks), chunkType))
            {
                chunks.Add((Chunks)chunkType, GetChunkData(reader, chunkLength, (Chunks)chunkType));


                // Skip the chunk data and CRC
                reader.BaseStream.Seek(4, SeekOrigin.Current);
            }
            else
                reader.BaseStream.Position++;
        }

        return chunks;
    }


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
    public static IChunk GetChunkData(BinaryReader reader, int chunkLength, Chunks chunkType)
    {
        List<byte> data = new();

        for (int i = 0; i < chunkLength; i++)
            data.Add(reader.ReadByte());

        switch (chunkType)
        {
            case Chunks.IHDR:
                return new IHDR_Chunk(data);
            case Chunks.PLTE:
                return new Generic_Chunk(data, chunkType);
            case Chunks.IDAT:
                return new Generic_Chunk(data, chunkType);
            case Chunks.IEND:
                return new Generic_Chunk(data, chunkType);
            case Chunks.pHYs:
                return new PHYS_Chunk(data);
            case Chunks.tEXt:
                return new Generic_Chunk(data, chunkType);
            case Chunks.zTXt:
                return new Generic_Chunk(data, chunkType);
            case Chunks.tIME:
                return new Generic_Chunk(data, chunkType);
            case Chunks.tRNS:
                return new Generic_Chunk(data, chunkType);
            case Chunks.cHRM:
                return new Generic_Chunk(data, chunkType);
            case Chunks.gAMA:
                return new Generic_Chunk(data, chunkType);
            case Chunks.iCCP:
                return new Generic_Chunk(data, chunkType);
            case Chunks.sBIT:
                return new Generic_Chunk(data, chunkType);
            case Chunks.sRGB:
                return new Generic_Chunk(data, chunkType);
            case Chunks.iTXt:
                return new Generic_Chunk(data, chunkType);
            case Chunks.bKGD:
                return new Generic_Chunk(data, chunkType);
            case Chunks.hIST:
                return new Generic_Chunk(data, chunkType);
            case Chunks.pCAL:
                return new Generic_Chunk(data, chunkType);
            case Chunks.sPLT:
                return new Generic_Chunk(data, chunkType);

        }

        return null;
    }


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

        var chunks = GetPngChunks(image);
        if (chunks.TryGetValue(Chunks.pHYs, out IChunk value))
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

        var chunks = GetPngChunks(image);
        if (chunks.TryGetValue(Chunks.pHYs, out IChunk value))
        {
            PHYS_Chunk physChunk = (PHYS_Chunk)value;
        }

        throw new ArgumentException("pHYs chunk not found in PNG file.");
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

        var chunks = GetPngChunks(image);
        return chunks.TryGetValue(Chunks.IDAT, out IChunk value) ? [.. value.Data] : ([]);
    }
}
