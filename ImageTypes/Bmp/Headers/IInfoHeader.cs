namespace LibImageUtilities.ImageTypes.Bmp.Headers;
public interface IInfoHeader
{
    public byte[] RawData { get; }
    public int RawDataLength { get; }

    public int Length { get; }
    public bool IsValid { get; }

    public int Size { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public short Planes { get; }
    public short BitCount { get; set; }
    public int Compression { get; set; }
    public CompressionMode CompressionEnum { get; }
    public int ImageSize { get; set; }
    public int XPixelsPerMeter { get; set; }
    public int YPixelsPerMeter { get; set; }
    public int ColorsUsed { get; set; }
    public int ColorsImportant { get; set; }
}
