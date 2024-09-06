using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;

namespace LibImageUtilities.ImageTypes.Bmp.Headers;

public enum CompressionMode
{
    RGB = 0,
    RLE8 = 1,
    RLE4 = 2,
    Bitfields = 3,
    JPEG = 4,
    PNG = 5,
    AlphaBitfields = 6,
    CMYK = 11,
    CMYKRLE8 = 12,
    CMYKRLE4 = 13
}

public class InfoHeader : CoreHeader
{
    public override int Length => 40;
    public override bool IsValid => _data.Count == Length && _data[0] == 0x28 && _data[1] == 0x00 && _data[2] == 0x00 && _data[3] == 0x00;

    private readonly byte[] DefaultData =
    [
        //Size
        0x28, 0x00, 0x00, 0x00,
        //Width
        0x00, 0x00, 0x00, 0x00,
        //Height
        0x00, 0x00, 0x00, 0x00,
        //Planes
        0x01, 0x00,
        //BitsPerPixel
        0x18, 0x00,
        //Compression
        0x00, 0x00, 0x00, 0x00,
        //ImageSize
        0x00, 0x00, 0x00, 0x00,
        //XPixelsPerMeter
        0x00, 0x00, 0x00, 0x00,
        //YPixelsPerMeter
        0x00, 0x00, 0x00, 0x00,
        //ColorsUsed
        0x00, 0x00, 0x00, 0x00,
        //ColorsImportant
        0x00, 0x00, 0x00, 0x00
    ];


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public PixelFormat PixelFormat
    {
        get
        {
            int bpp = BitCount;
            return bpp == 16 && Compression == 3
                ? PixelFormat.Format16bppRgb565
                : bpp == 16 && Compression == 6
                ? PixelFormat.Format16bppArgb1555
                : bpp == 32 && Compression == 3
                ? PixelFormat.Format32bppRgb
                : bpp == 32 && Compression == 6
                ? PixelFormat.Format32bppPArgb
                : bpp == 64 && Compression == 6
                ? PixelFormat.Format64bppPArgb
                : bpp switch
                {
                    1 => PixelFormat.Format1bppIndexed,
                    4 => PixelFormat.Format4bppIndexed,
                    8 => PixelFormat.Format8bppIndexed,
                    16 => PixelFormat.Format16bppRgb555,
                    24 => PixelFormat.Format24bppRgb,
                    32 => PixelFormat.Format32bppArgb,
                    48 => PixelFormat.Format48bppRgb,
                    64 => PixelFormat.Format64bppArgb,
                    _ => PixelFormat.Undefined
                };
        }
    }

    public override int Compression
    {
        get => BitConverter.ToInt32(_data.Skip(16).Take(4).ToArray(), 0);
        set
        {
            byte[] sizeBytes = BitConverter.GetBytes(value);
            _data[16] = sizeBytes[0];
            _data[17] = sizeBytes[1];
            _data[18] = sizeBytes[2];
            _data[19] = sizeBytes[3];
        }
    }
    public override CompressionMode CompressionEnum => (CompressionMode)Compression;
    public override int ImageSize
    {
        get => BitConverter.ToInt32(_data.Skip(20).Take(4).ToArray(), 0);
        set
        {
            byte[] sizeBytes = BitConverter.GetBytes(value);
            _data[20] = sizeBytes[0];
            _data[21] = sizeBytes[1];
            _data[22] = sizeBytes[2];
            _data[23] = sizeBytes[3];
        }
    }
    public override int XPixelsPerMeter
    {
        get => BitConverter.ToInt32(_data.Skip(24).Take(4).ToArray(), 0);
        set
        {
            byte[] sizeBytes = BitConverter.GetBytes(value);
            _data[24] = sizeBytes[0];
            _data[25] = sizeBytes[1];
            _data[26] = sizeBytes[2];
            _data[27] = sizeBytes[3];
        }
    }
    public override int YPixelsPerMeter
    {
        get => BitConverter.ToInt32(_data.Skip(28).Take(4).ToArray(), 0);
        set
        {
            byte[] sizeBytes = BitConverter.GetBytes(value);
            _data[28] = sizeBytes[0];
            _data[29] = sizeBytes[1];
            _data[30] = sizeBytes[2];
            _data[31] = sizeBytes[3];
        }
    }
    public override int ColorsUsed
    {
        get => BitConverter.ToInt32(_data.Skip(32).Take(4).ToArray(), 0);
        set
        {
            byte[] sizeBytes = BitConverter.GetBytes(value);
            _data[32] = sizeBytes[0];
            _data[33] = sizeBytes[1];
            _data[34] = sizeBytes[2];
            _data[35] = sizeBytes[3];
        }
    }
    public override int ColorsImportant
    {
        get => BitConverter.ToInt32(_data.Skip(36).Take(4).ToArray(), 0);
        set
        {
            byte[] sizeBytes = BitConverter.GetBytes(value);
            _data[36] = sizeBytes[0];
            _data[37] = sizeBytes[1];
            _data[38] = sizeBytes[2];
            _data[39] = sizeBytes[3];
        }
    }

    public InfoHeader() => _data = [.. DefaultData];
    public InfoHeader(byte[] data) => _data = [.. data];
    public InfoHeader(List<byte> data) => _data = data;
}
