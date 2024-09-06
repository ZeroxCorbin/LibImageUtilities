using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibImageUtilities.ImageTypes.Bmp;



public struct ColorEntry
{
    public byte Blue;
    public byte Green;
    public byte Red;
    public byte Reserved;
}

public enum ColorChannel
{
    Blue = 0,
    Green = 1,
    Red = 2,
    Reserved = 3
}

public class ColorTable
{
    private readonly List<ColorEntry> _data;
    public byte[] RawData => _data.SelectMany(c => new byte[] { c.Blue, c.Green, c.Red, c.Reserved }).ToArray();
    public int RawDataLength => _data.Count * 4;

    public int Count => _data.Count;

    public ColorTable(int length)
    {
        _data = new List<ColorEntry>(length);
    }

    public ColorTable(byte[] data)
    {
        _data = [];
        for (int i = 0; i < data.Length; i += 4)
        {
            _data.Add(new ColorEntry
            {
                Blue = data[i],
                Green = data[i + 1],
                Red = data[i + 2],
                Reserved = data[i + 3]
            });
        }
    }

    public ColorTable(List<ColorEntry> data)
    {
        _data = data;
    }

    public ColorEntry this[int index]
    {
        get => _data[index];
        set => _data[index] = value;
    }

    public void Add(ColorEntry entry)
    {
        _data.Add(entry);
    }

    public void RemoveAt(int index)
    {
        _data.RemoveAt(index);
    }

    public void Clear()
    {
        _data.Clear();
    }
}
