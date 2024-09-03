using System;
using System.Collections;

namespace LibImageUtilities.ImageTypes.Png;

public class Parameter(int length, ChunkTypes type, bool multipleAllowed, double ordering)
{
    public int Length { get; } = length;
    public ChunkTypes Type { get; } = type;
    public bool MultipleAllowed { get; } = multipleAllowed;
    public double Ordering { get; } = ordering;

    public bool Critical => new BitArray((int)Type)[5];
    public bool Public => new BitArray((int)Type)[13];
    public bool Reserved => new BitArray((int)Type)[21];
    public bool SafeToCopy => new BitArray((int)Type)[29];
}

public static class ChunkParameters
{
    public static Parameter GetParameter(ChunkTypes chunk) => chunk switch
    {
        ChunkTypes.IHDR => new Parameter(13, ChunkTypes.IHDR, false, 0),
        ChunkTypes.PLTE => new Parameter(0, ChunkTypes.PLTE, false, 1),
        ChunkTypes.IDAT => new Parameter(0, ChunkTypes.IDAT, true, 2),
        ChunkTypes.IEND => new Parameter(0, ChunkTypes.IEND, false, 3),
        ChunkTypes.pHYs => new Parameter(9, ChunkTypes.pHYs, false, 0.5),
        ChunkTypes.tEXt => new Parameter(0, ChunkTypes.tEXt, true, 2.5),
        ChunkTypes.zTXt => new Parameter(0, ChunkTypes.zTXt, true, 2.5),
        ChunkTypes.tIME => new Parameter(7, ChunkTypes.tIME, false, 2.5),
        ChunkTypes.cHRM => new Parameter(32, ChunkTypes.cHRM, false, 0.5),
        ChunkTypes.gAMA => new Parameter(4, ChunkTypes.gAMA, false, 0.5),
        ChunkTypes.iCCP => new Parameter(0, ChunkTypes.iCCP, false, 0.5),
        ChunkTypes.sBIT => new Parameter(0, ChunkTypes.sBIT, false, 0.5),
        ChunkTypes.sRGB => new Parameter(1, ChunkTypes.sRGB, false, 0.5),
        ChunkTypes.iTXt => new Parameter(0, ChunkTypes.iTXt, true, 2.5),
        ChunkTypes.bKGD => new Parameter(0, ChunkTypes.bKGD, false, 1.5),
        ChunkTypes.hIST => new Parameter(0, ChunkTypes.hIST, false, 1.5),
        ChunkTypes.tRNS => new Parameter(0, ChunkTypes.tRNS, false, 1.5),
        ChunkTypes.pCAL => new Parameter(0, ChunkTypes.pCAL, false, 2.5),
        ChunkTypes.sPLT => new Parameter(0, ChunkTypes.sPLT, false, 0.5),
        _ => throw new ArgumentException("Unsupported chunk type.")
    };
}
