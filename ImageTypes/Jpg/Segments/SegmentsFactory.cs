using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibImageUtilities.ImageTypes.Jpg.Segments;
public static class SegmentsFactory
{
    public static ISegment GetSegment(MarkerTypes type, List<byte> data)
    {
        if (type == MarkerTypes.APP0)
        {
            string identifier = Encoding.ASCII.GetString(data.Skip(2).Take(5).ToArray());
            if (identifier == "JFIF")
                return new Segments.JFIF_APP0(data);
            else if (identifier == "JFXX")
                return new Segments.JFXX_APP0(data);
        }
        return new Segments.GenericSegment(type, data);
    }
}