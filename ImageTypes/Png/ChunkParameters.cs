using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibImageUtilities.ImageTypes.Png;

public class Parameter
{
    public ChunkTypes Type { get; set; }
    public bool Critical { get; set; }
    public bool MultipleAllowed { get; set; }
    public int Length { get; set; }
}

public static class ChunkParameters
{

    public static Parameter GetParameter(ChunkTypes chunk) => chunk switch
    {
        ChunkTypes.IHDR => new Parameter() { Type = ChunkTypes.IHDR, Critical = true, MultipleAllowed = false, Length = 13 },
        ChunkTypes.PLTE => new Parameter() { Type = ChunkTypes.PLTE, Critical = false, MultipleAllowed = false, Length = 0 },
        ChunkTypes.IDAT => new Parameter() { Type = ChunkTypes.IDAT, Critical = true, MultipleAllowed = true, Length = 0 },
        ChunkTypes.IEND => new Parameter() { Type = ChunkTypes.IEND, Critical = true, MultipleAllowed = false, Length = 0 },
        ChunkTypes.pHYs => new Parameter() { Type = ChunkTypes.pHYs, Critical = false, MultipleAllowed = false, Length = 9 },
        ChunkTypes.tEXt => new Parameter() { Type = ChunkTypes.tEXt, Critical = false, MultipleAllowed = true, Length = 0 },
        ChunkTypes.zTXt => new Parameter() { Type = ChunkTypes.zTXt, Critical = false, MultipleAllowed = true, Length = 0 },
        ChunkTypes.tIME => new Parameter() { Type = ChunkTypes.tIME, Critical = false, MultipleAllowed = false, Length = 7 },
        ChunkTypes.tRNS => new Parameter() { Type = ChunkTypes.tRNS, Critical = false, MultipleAllowed = false, Length = 0 },
        ChunkTypes.cHRM => new Parameter() { Type = ChunkTypes.cHRM, Critical = false, MultipleAllowed = false, Length = 32 },
        ChunkTypes.gAMA => new Parameter() { Type = ChunkTypes.gAMA, Critical = false, MultipleAllowed = false, Length = 4 },
        ChunkTypes.iCCP => new Parameter() { Type = ChunkTypes.iCCP, Critical = false, MultipleAllowed = false, Length = 0 },
        ChunkTypes.sBIT => new Parameter() { Type = ChunkTypes.sBIT, Critical = false, MultipleAllowed = false, Length = 0 },
        ChunkTypes.sRGB => new Parameter() { Type = ChunkTypes.sRGB, Critical = false, MultipleAllowed = false, Length = 1 },
        ChunkTypes.iTXt => new Parameter() { Type = ChunkTypes.iTXt, Critical = false, MultipleAllowed = true, Length = 0 },
        ChunkTypes.bKGD => new Parameter() { Type = ChunkTypes.bKGD, Critical = false, MultipleAllowed = false, Length = 0 },
        ChunkTypes.hIST => new Parameter() { Type = ChunkTypes.hIST, Critical = false, MultipleAllowed = false, Length = 0 },
        ChunkTypes.pCAL => new Parameter() { Type = ChunkTypes.pCAL, Critical = false, MultipleAllowed = false, Length = 0 },
        ChunkTypes.sPLT => new Parameter() { Type = ChunkTypes.sPLT, Critical = false, MultipleAllowed = false, Length = 0 },
        _ => throw new ArgumentException("Unsupported chunk type.")
    };

}
