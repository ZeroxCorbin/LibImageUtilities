using System;
using System.Collections.Generic;
using System.Linq;

namespace LibImageUtilities.ImageTypes.Png;
public class Signature
{
    private readonly List<byte> _data;

    public byte[] RawData => _data.ToArray();

    public const int Length = 8;

    public bool IsValid => _data.Count >= 8 &&
        _data[0] == 0x89 && //Non-ASCII unique ID
                            //The first byte is chosen as a non-ASCII value to reduce the probability that a text file may be misrecognized as a PNG file; also, it catches bad file transfers that clear bit 7
        _data[1] == 0x50 && //ASCII 'P' considered part of the unique ID
        _data[2] == 0x4E && //Bytes two [1] through four [3] name the format. ASCII 'P' 'N' 'G'
        _data[3] == 0x47 &&
        _data[4] == 0x0D && //The CR-LF sequence catches bad file transfers that alter newline sequences.
        _data[5] == 0x0A &&
        _data[6] == 0x1A && //The control-Z character stops file display under MS-DOS.
        _data[7] == 0x0A;   //The final line feed checks for the inverse of the CR-LF translation problem.
    public int UniqueID => BitConverter.ToInt16(_data.Take(2).ToArray(), 0);
    public string FormatName => System.Text.Encoding.Default.GetString(_data.Skip(1).Take(3).ToArray());
    public bool ValidCrLf => _data[4] == 0x0D && _data[5] == 0x0A;
    public bool ValidControlZ => _data[6] == 0x1A;
    public bool ValidFinalLf => _data[7] == 0x0A;

    public Signature(byte[] data)
    {
        _data = data.ToList();
    }
    public Signature()
    {
        _data = new List<byte>()
        {
            { 0x89 },
            { 0x50 },
            { 0x4E },
            { 0x47 },
            { 0x0D },
            { 0x0A },
            { 0x1A },
            { 0x0A }
        };
    }
}