using System;
using System.Collections.Generic;
using System.Linq;

namespace LibImageUtilities.ImageTypes.Bmp.Headers;
public class CoreHeader : IInfoHeader
{
    public virtual int Length => 12;
    public virtual bool IsValid => _data.Count == Length && _data[0] == 0x0C && _data[1] == 0x00;

    private readonly byte[] DefaultData =
    {
        //Size
        0x00, 0x00, 0x00, 0x00,
        //Width
        0x00, 0x00, 0x00, 0x00,
        //Height
        0x00, 0x00, 0x00, 0x00
    };

    internal List<byte> _data;
    public byte[] RawData => [.. _data];
    public int RawDataLength => _data.Count;

    public int Size
    {
        get => BitConverter.ToInt32(_data.Skip(0).Take(4).ToArray(), 0);
        set
        {
            byte[] sizeBytes = BitConverter.GetBytes(value);
            _data[0] = sizeBytes[0];
            _data[1] = sizeBytes[1];
            _data[2] = sizeBytes[2];
            _data[3] = sizeBytes[3];
        }
    }
    public int Width
    {
        get => BitConverter.ToInt32(_data.Skip(4).Take(4).ToArray(), 0);
        set
        {
            byte[] sizeBytes = BitConverter.GetBytes(value);
            _data[4] = sizeBytes[0];
            _data[5] = sizeBytes[1];
            _data[6] = sizeBytes[2];
            _data[7] = sizeBytes[3];
        }
    }
    public int Height
    {
        get => BitConverter.ToInt32(_data.Skip(8).Take(4).ToArray(), 0);
        set
        {
            byte[] sizeBytes = BitConverter.GetBytes(value);
            _data[8] = sizeBytes[0];
            _data[9] = sizeBytes[1];
            _data[10] = sizeBytes[2];
            _data[11] = sizeBytes[3];
        }
    }
    public short Planes
    {
        get => BitConverter.ToInt16(_data.Skip(12).Take(2).ToArray(), 0);
        set
        {
            byte[] sizeBytes = BitConverter.GetBytes(value);
            _data[12] = sizeBytes[0];
            _data[13] = sizeBytes[1];
        }
    }
    public short BitCount
    {
        get => BitConverter.ToInt16(_data.Skip(14).Take(2).ToArray(), 0);
        set
        {
            byte[] sizeBytes = BitConverter.GetBytes(value);
            _data[14] = sizeBytes[0];
            _data[15] = sizeBytes[1];
        }
    }

    //Not apart of the core header but is apart of the Interface
    public virtual int Compression { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public virtual CompressionMode CompressionEnum => throw new NotImplementedException();
    public virtual int ImageSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public virtual int XPixelsPerMeter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public virtual int YPixelsPerMeter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public virtual int ColorsUsed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public virtual int ColorsImportant { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


    public CoreHeader() => _data = [.. DefaultData];
    public CoreHeader(byte[] data) => _data = [.. data];
    public CoreHeader(List<byte> data) => _data = data;
}
