using System.Collections.Generic;

namespace LibImageUtilities.ImageTypes.Jpg.Segments;

public interface ISegment
{
    public MarkerTypes Flag { get; }
    public int Length { get; }
    public List<byte> Data { get; }
    public byte[] RawData { get; }
}

