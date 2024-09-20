using LibImageUtilities.ImageTypes.Jpg.Segments;
using System;
using System.Collections.Generic;
using System.IO;

namespace LibImageUtilities.ImageTypes.Jpg;
public class Utilities
{
    public static bool IsJpg(byte[] img) => img.Length > 2 && img[0] == 0xFF && img[1] == 0xD8;

    public static Dictionary<MarkerTypes, ISegment> GetMarkers(byte[] img)
    {
        Dictionary<MarkerTypes, ISegment> markers = new Dictionary<MarkerTypes, ISegment>();

        using MemoryStream ms = new(img);
        using BinaryReader reader = new(ms);

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            byte markerStart = reader.ReadByte();
            byte markerFlag = reader.ReadByte();

            if (markerStart != 0xFF)
                continue;

            if (Enum.IsDefined(typeof(MarkerTypes), markerFlag))
            {
                MarkerTypes markerType = (MarkerTypes)markerFlag;
                ISegment markerData = GetSegmentData(reader, markerType);
                markers.Add(markerType, markerData);
            }
        }

        return markers;
    }
    private static ISegment GetSegmentData(BinaryReader reader, MarkerTypes markerType)
    {
        List<byte> data = [];

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            byte b = reader.ReadByte();
            if (b == 0xFF)
            {
                byte nextByte = reader.ReadByte();
                if (nextByte == 0x00)
                {
                    data.Add(b);
                    data.Add(nextByte);
                    continue;
                }
                else
                {
                    reader.BaseStream.Position -= 2;
                    break;
                }
            }
            data.Add(b);
        }

        return SegmentsFactory.GetSegment(markerType, data);
    }
}
