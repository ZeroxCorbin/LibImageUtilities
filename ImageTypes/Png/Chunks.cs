﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace LibImageUtilities.ImageTypes.Png;

public enum ChunkTypes
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

public interface IChunk
{
    public const int CrcSize = 4;

    int Length { get; }
    ChunkTypes Type { get; }
    Parameter Parameters { get; }
    List<byte> Data { get; }
    byte[] RawData { get; }
    uint CRC { get; }
    bool CheckCRC(uint crc);
}

public class Generic_Chunk : IChunk
{
    private List<byte> _data = [];
    private byte[] crcData
    {
        get
        {
            var first = BitConverter.GetBytes((int)Type).Reverse().ToList();
            first.AddRange(_data);
            return first.ToArray();
        }
    }

    public ChunkTypes Type { get; }
    public Parameter Parameters => ChunkParameters.GetParameter(Type);

    public int Length => _data.Count;
    public List<byte> Data => _data;
    public byte[] RawData
    {
        get
        {
            // Add the chunk length
            var data = new List<byte>(BitConverter.GetBytes(_data.Count).Reverse().ToArray());
            // Add the chunk type
            data.AddRange(BitConverter.GetBytes((int)Type).Reverse().ToArray());
            // Add the chunk data
            data.AddRange(_data);
            // Add the CRC
            data.AddRange(BitConverter.GetBytes(CRC).Reverse());
            return [.. data];
        }
    }

    public uint CRC { get { var data = crcData; return CRC32.ComputeCrc(data, data.Length); } }
    public bool CheckCRC(uint crc) => crc == CRC;

    public Generic_Chunk(List<byte> chunk, ChunkTypes type)
    {
        Type = type;

        var crc = BitConverter.ToInt32(chunk.Skip(chunk.Count - IChunk.CrcSize).Reverse().ToArray(), 0);
        _data.AddRange(chunk.GetRange(0, chunk.Count - IChunk.CrcSize));

        if (!CheckCRC((uint)crc))
            throw new ArgumentException("CRC check failed.");
    }
}

public class IHDR_Chunk : IChunk
{
    private List<byte> _data = [];
    private byte[] crcData
    {
        get
        {
            var first = BitConverter.GetBytes((int)Type).Reverse().ToList();
            first.AddRange(_data);
            return first.ToArray();
        }
    }

    public ChunkTypes Type => ChunkTypes.IHDR;
    public Parameter Parameters => ChunkParameters.GetParameter(Type);

    public int Length => _data.Count;
    public List<byte> Data => _data;
    public byte[] RawData
    {
        get
        {
            // Add the chunk length
            var data = new List<byte>(BitConverter.GetBytes(_data.Count).Reverse().ToArray());
            // Add the chunk type
            data.AddRange(BitConverter.GetBytes((int)Type).Reverse().ToArray());
            // Add the chunk data
            data.AddRange(_data);
            // Add the CRC
            data.AddRange(BitConverter.GetBytes(CRC).Reverse());
            return [.. data];
        }
    }

    public uint CRC { get { var data = crcData; return CRC32.ComputeCrc(data, data.Length); } }
    public bool CheckCRC(uint crc) => crc == CRC;

    public int Width
    {
        get => BitConverter.ToInt32(_data.Take(4).Reverse().ToArray(), 0);
        set
        {
            _data[0] = (byte)(value >> 24);
            _data[1] = (byte)(value >> 16);
            _data[2] = (byte)(value >> 8);
            _data[3] = (byte)value;
        }
    }
    public int Height
    {
        get => BitConverter.ToInt32(_data.Skip(4).Take(4).Reverse().ToArray(), 0);
        set
        {
            _data[4] = (byte)(value >> 24);
            _data[5] = (byte)(value >> 16);
            _data[6] = (byte)(value >> 8);
            _data[7] = (byte)value;
        }
    }
    public byte BitDepth
    {
        get => _data[8];
        set => _data[8] = value;
    }
    public byte ColorType
    {
        get => _data[9];
        set => _data[9] = value;
    }
    public byte CompressionMethod
    {
        get => _data[10];
        set => _data[10] = value;
    }
    public byte FilterMethod
    {
        get => _data[11];
        set => _data[11] = value;
    }
    public byte InterlaceMethod
    {
        get => _data[12];
        set => _data[12] = value;
    }

    public IHDR_Chunk(List<byte> chunk)
    {
        var crc = BitConverter.ToInt32(chunk.Skip(chunk.Count - IChunk.CrcSize).Reverse().ToArray(), 0);
        _data.AddRange(chunk.GetRange(0, chunk.Count - IChunk.CrcSize));

        if (!CheckCRC((uint)crc))
            throw new ArgumentException("CRC check failed.");
    }
    public IHDR_Chunk(int width, int height, byte bitDepth, byte colorType, byte compressionMethod, byte filterMethod, byte interlaceMethod)
    {
        _data.AddRange(BitConverter.GetBytes(width).Reverse());
        _data.AddRange(BitConverter.GetBytes(height).Reverse());
        _data.Add(bitDepth);
        _data.Add(colorType);
        _data.Add(compressionMethod);
        _data.Add(filterMethod);
        _data.Add(interlaceMethod);
    }
}
/// <summary>
/// Describes the image pixel dimesions.
/// </summary>
public class PHYS_Chunk : IChunk
{
    private List<byte> _data = [];
    private byte[] crcData
    {
        get
        {
            var first = BitConverter.GetBytes((int)Type).Reverse().ToList();
            first.AddRange(_data);
            return first.ToArray();
        }
    }
    public ChunkTypes Type => ChunkTypes.pHYs;
    public Parameter Parameters => ChunkParameters.GetParameter(Type);

    public int Length => _data.Count;
    public List<byte> Data => _data;
    public byte[] RawData
    {
        get
        {
            // Add the chunk length
            var data = new List<byte>(BitConverter.GetBytes(_data.Count).Reverse().ToArray());
            // Add the chunk type
            data.AddRange(BitConverter.GetBytes((int)Type).Reverse().ToArray());
            // Add the chunk data
            data.AddRange(_data);
            // Add the CRC
            data.AddRange(BitConverter.GetBytes(CRC).Reverse());
            return [.. data];
        }
    }

    public uint CRC { get { var data = crcData; return CRC32.ComputeCrc(data, data.Length); } }
    public bool CheckCRC(uint crc) => crc == CRC;

    public int PixelsPerUnitX
    {
        get => BitConverter.ToInt32(_data.Take(4).Reverse().ToArray(), 0);
        set
        {
            _data[0] = (byte)(value >> 24);
            _data[1] = (byte)(value >> 16);
            _data[2] = (byte)(value >> 8);
            _data[3] = (byte)value;
        }
    }
    public int PixelsPerUnitY
    {
        get => BitConverter.ToInt32(_data.Skip(4).Take(4).Reverse().ToArray(), 0);
        set
        {
            _data[4] = (byte)(value >> 24);
            _data[5] = (byte)(value >> 16);
            _data[6] = (byte)(value >> 8);
            _data[7] = (byte)value;
        }
    }
    public byte UnitSpecifier
    {
        get => _data[8];
        set => _data[8] = value;
    }

    public int DpiX
    {
        get => UnitSpecifier == 0x01 ? ImageUtilities.Dpm2Dpi(PixelsPerUnitX) : 0;
        set => PixelsPerUnitX = ImageUtilities.Dpi2Dpm(value);
    }
    public int DpiY
    {
        get => UnitSpecifier == 0x01 ? ImageUtilities.Dpm2Dpi(PixelsPerUnitY) : 0;
        set => PixelsPerUnitY = ImageUtilities.Dpi2Dpm(value);
    }
    /// <summary>
    /// Initilize with 96x96 DPI
    /// </summary>
    public PHYS_Chunk()
    {
        _data.AddRange(new byte[9]);

        UnitSpecifier = 0x01;
        DpiX = 96;
        DpiY = 96;
    }
    public PHYS_Chunk(List<byte> chunk)
    {
        var crc = BitConverter.ToInt32(chunk.Skip(chunk.Count - IChunk.CrcSize).Reverse().ToArray(), 0);
        _data.AddRange(chunk.GetRange(0, chunk.Count - IChunk.CrcSize));

        if (!CheckCRC((uint)crc))
            throw new ArgumentException("CRC check failed.");
    }
    public PHYS_Chunk(int pixelsPerUnitX, int pixelsPerUnitY, byte unitSpecifier)
    {
        _data.AddRange(BitConverter.GetBytes(pixelsPerUnitX).Reverse());
        _data.AddRange(BitConverter.GetBytes(pixelsPerUnitY).Reverse());
        _data.Add(unitSpecifier);
    }
    /// <summary>
    /// Initialize with UnitSpecifier = 0x01
    /// </summary>
    /// <param name="dpiX"></param>
    /// <param name="dpiY"></param>
    public PHYS_Chunk(int dpiX, int dpiY)
    {
        _data.AddRange(new byte[9]);

        UnitSpecifier = 0x01;
        DpiX = dpiX;
        DpiY = dpiY;
    }
}

public class IDAT_Chunk : IChunk
{
    private List<byte> _data = [];
    private byte[] crcData
    {
        get
        {
            var first = BitConverter.GetBytes((int)Type).Reverse().ToList();
            first.AddRange(_data);
            return first.ToArray();
        }
    }

    public ChunkTypes Type => ChunkTypes.IDAT;
    public Parameter Parameters => ChunkParameters.GetParameter(Type);

    public int Length => _data.Count;
    public List<byte> Data => _data;
    public byte[] RawData
    {
        get
        {
            // Add the chunk length
            var data = new List<byte>(BitConverter.GetBytes(_data.Count).Reverse().ToArray());
            // Add the chunk type
            data.AddRange(BitConverter.GetBytes((int)Type).Reverse().ToArray());
            // Add the chunk data
            data.AddRange(_data);
            // Add the CRC
            data.AddRange(BitConverter.GetBytes(CRC).Reverse());
            return [.. data];
        }
    }

    public uint CRC { get { var data = crcData; return CRC32.ComputeCrc(data, data.Length); } }
    public bool CheckCRC(uint crc) => crc == CRC;

    /// <summary>
    /// Initialize with the provided chunk. hasCRC must be true.
    /// Must Include 4 byte CRC at the end of the chunk.
    /// This requirement is so there can be an initializer with just data.
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="hasCRC"></param>
    /// <exception cref="ArgumentException"></exception>
    public IDAT_Chunk(List<byte> chunk, bool hasCRC)
    {
        if (!hasCRC)
            throw new ArgumentException("The provided data does not include the 4 byte CRC.");

        var crc = BitConverter.ToInt32(chunk.Skip(chunk.Count - IChunk.CrcSize).Reverse().ToArray(), 0);
        _data.AddRange(chunk.GetRange(0, chunk.Count - IChunk.CrcSize));

        if (!CheckCRC((uint)crc))
            throw new ArgumentException("CRC check failed.");
    }
    /// <summary>
    /// Initialize with the provided data.
    /// Must NOT Include 4 byte CRC at the end of the data.
    /// </summary>
    /// <param name="data"></param>
    public IDAT_Chunk(List<byte> data)
    {
        _data.AddRange(data);
    }
}

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
