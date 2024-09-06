using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotImaging;
using LibImageUtilities.ImageTypes.Bmp.Headers;

namespace LibImageUtilities.ImageTypes.Bmp;

public class Bmp
{
    public FileHeader Header { get; private set; }
    public IInfoHeader InfoHeader { get; private set; }
    public ColorTable? ColorTable { get; private set; }

    private List<byte> _imageData;
    public byte[] ImageData => _imageData.ToArray();

    public byte[] RawData => Header.RawData
        .Concat(InfoHeader.RawData)
        .Concat(ColorTable?.RawData ?? [])
        .Concat(ImageData)
        .ToArray();

    private Bmp(byte[] image, bool converted) => Parse(image);
    public Bmp(byte[] image) : this(Utilities.GetBmp(image), true) { }
    public Bmp(string path) : this(Utilities.GetBmp(File.ReadAllBytes(path)), true) { }
    public Bmp(Stream stream) : this(Utilities.GetBmp(stream), true) { }

    private void Parse(byte[] image)
    {
        Header = new FileHeader(image.Take(FileHeader.Length).ToArray());

        var headerSize = BitConverter.ToInt32(image.Skip(FileHeader.Length).Take(4).ToArray(), 0);
        InfoHeader = 
            headerSize == 12
            ? new CoreHeader(image.Skip(FileHeader.Length).Take(headerSize).ToArray())
            : headerSize == 40
            ? new InfoHeader(image.Skip(FileHeader.Length).Take(headerSize).ToArray())
            : headerSize == 108
            ? new InfoHeaderV4(image.Skip(FileHeader.Length).Take(headerSize).ToArray())
            : headerSize == 124
            ? new InfoHeaderV5(image.Skip(FileHeader.Length).Take(headerSize).ToArray())
            : throw new Exception("Header size is not valid.");

        if (InfoHeader.BitCount <= 8)
        {
            int colorTableLength = (int)Math.Pow(2, InfoHeader.BitCount) * 4;
            ColorTable = new ColorTable(image.Skip(FileHeader.Length + InfoHeader.Length).Take(colorTableLength).ToArray());
        }

        _imageData = image.Skip(Header.DataOffset).ToList();
    }

    /// <summary>
    /// Confirm Header values are correct.
    /// Confirm the image data length is correct.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void Confirm()
    {
        if (!Header.IsValid)
            throw new Exception("Header is not valid.");

        if (Header.Signature != "BM")
            throw new Exception("Signature is not 'BM'.");

        if (Header.DataOffset != Header.RawDataLength + InfoHeader.RawDataLength + (ColorTable?.RawDataLength ?? 0))
            throw new Exception("Data offset does not match expected value.");

        if (Header.FileSize != Header.DataOffset + _imageData.Count)
            throw new Exception("File size does not match expected value.");

        if (!InfoHeader.IsValid)
            throw new Exception("Info Header is not valid.");

        if (InfoHeader.Width <= 0 || InfoHeader.Height <= 0)
            throw new Exception("Width or height is less than or equal to 0.");

        if (InfoHeader.BitCount <= 0)
            throw new Exception("Bits per pixel is less than or equal to 0.");

        if (!Enum.IsDefined(typeof(CompressionMode), InfoHeader.Compression))
            throw new Exception("Compression mode is not valid.");
        
        if(InfoHeader.Length > 12)
        {
            if (InfoHeader.CompressionEnum == CompressionMode.RLE8)
            {
                if (InfoHeader.BitCount != 8)
                    throw new Exception("Compression mode is RLE8 but bits per pixel is not 8.");

                if (InfoHeader.ImageSize != 0)
                    throw new Exception("Compression mode is RLE8 but image size is not 0.");

                if (InfoHeader.ColorsUsed != 0)
                    throw new Exception("Compression mode is RLE8 but colors used is not 0.");

                if (InfoHeader.ColorsImportant != 0)
                    throw new Exception("Compression mode is RLE8 but colors important is not 0.");
            }


            if (InfoHeader.CompressionEnum == CompressionMode.RLE4 && InfoHeader.BitCount != 4)
                throw new Exception("Compression mode is RLE4 but bits per pixel is not 4.");
        }

        int expectedLength = InfoHeader.Width * InfoHeader.Height * InfoHeader.BitCount / 8;
        if (_imageData.Count != expectedLength)
            throw new Exception("Image data length does not match expected length.");
    }

    public static byte[] Get(string path) => Utilities.GetBmp(File.ReadAllBytes(path));
    public static byte[] Get(Stream stream) => Utilities.GetBmp(stream);
    public static byte[] Get(byte[] data) => Utilities.GetBmp(data);

}