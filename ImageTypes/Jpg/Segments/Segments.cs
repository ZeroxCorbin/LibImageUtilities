using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibImageUtilities.ImageTypes.Jpg.Segments;

public class Segments
{
    public const byte MarkerStart = 0xFF;

    public class GenericSegment : ISegment
    {
        internal List<byte> _data = [];

        public MarkerTypes Flag { get; }
        public int Length => _data.Count > 2 ? BitConverter.ToUInt16(_data.Skip(0).Take(2).ToArray(), 0) : 0;
        public List<byte> Data => _data;
        public byte[] RawData
        {
            get
            {
                // Add marker start
                var data = new List<byte>()
                {
                    MarkerStart,
                    (byte)Flag
                };

                if (_data != null && _data.Count > 0)
                    data.AddRange(_data);

                return [.. data];
            }
        }

        public GenericSegment(MarkerTypes flag, List<byte> data)
        {
            Flag = flag;
            _data = data;
        }
    }

    public class Base_APP0 : GenericSegment
    {
        public bool IsJFIF => Identifier == "JFIF";
        public bool IsJFXX => Identifier == "JFXX";
        public string Identifier => Encoding.ASCII.GetString(_data.Skip(2).Take(5).ToArray());
        public Base_APP0(List<byte> data) : base(MarkerTypes.APP0, data) { }

    }

    public class JFIF_APP0 : Base_APP0
    {
        public string Version => $"{_data[7]}.{_data[8]}";
        public byte Units => _data[9];
        public ushort XDensity => BitConverter.ToUInt16(_data.Skip(10).Take(2).ToArray(), 0);
        public ushort YDensity => BitConverter.ToUInt16(_data.Skip(12).Take(2).ToArray(), 0);
        public byte XThumbnail => _data[14];
        public byte YThumbnail => _data[15];
        public List<byte> Thumbnail => _data.Skip(16).ToList();

        public JFIF_APP0(List<byte> data) : base(data) { }
    }

    public class JFXX_APP0 : Base_APP0
    {
        public ThumbnailFormats ThumbnailFormat => (ThumbnailFormats)_data[7];

        public List<byte> Thumbnail
        {
            get
            {
                if (ThumbnailFormat == ThumbnailFormats.Unknown)
                    return [];
                else if (ThumbnailFormat == ThumbnailFormats.JPEG)
                    return _data.Skip(8).ToList();
                else if (ThumbnailFormat == ThumbnailFormats.Palette)
                    return _data.Skip(778).ToList();
                else if (ThumbnailFormat == ThumbnailFormats.RGB)
                    return _data.Skip(10).ToList();
                else
                    return [];
            }
        }
        public byte[] ThumbnailPallet => ThumbnailFormat switch
        {
            ThumbnailFormats.Palette => _data.Skip(10).Take(768).ToArray(),
            _ => [],
        };
        public int XThumbnail => ThumbnailFormat switch
        {
            ThumbnailFormats.Palette => _data[8],
            ThumbnailFormats.RGB => _data[8],
            _ => 0,
        };
        public int YThumbnail => ThumbnailFormat switch
        {
            ThumbnailFormats.Palette => _data[9],
            ThumbnailFormats.RGB => _data[9],
            _ => 0,
        };

        public JFXX_APP0(List<byte> data) : base(data) { }
    }

}
