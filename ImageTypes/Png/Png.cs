using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibImageUtilities.ImageTypes.Png;
public class Png
{
    public const int IntSize = 4;

    public Signature Signature { get; }
    public Dictionary<ChunkTypes, IChunk> Chunks { get; }

    public IHDR_Chunk? IHDR
    {
        get => Chunks.TryGetValue(ChunkTypes.IHDR, out IChunk? val) ? (IHDR_Chunk)val : null;
        set
        {
            if (value == null)
                return;

            if (Chunks.TryGetValue(ChunkTypes.IHDR, out IChunk? val))
                Chunks[ChunkTypes.IHDR] = value;
            else
                Chunks.Add(ChunkTypes.IHDR, value);
        }
    }
    public PHYS_Chunk? pHYs
    {
        get => Chunks.TryGetValue(ChunkTypes.pHYs, out IChunk? val) ? (PHYS_Chunk)val : null;
        set
        {
            if (value == null)
            {
                _ = Chunks.Remove(ChunkTypes.pHYs);
                return;
            }

            if (Chunks.TryGetValue(ChunkTypes.pHYs, out IChunk? val))
                Chunks[ChunkTypes.pHYs] = value;
            else
                Chunks.Add(ChunkTypes.pHYs, value);
        }
    }


    public Png(byte[] image)
    {
        Signature = new Signature(image.Take(Signature.Length).ToArray());
        Chunks = Utilities.GetChunks(image);
    }

    public byte[] GetBytes()
    {
        List<byte> data = new(Signature.RawData);

        var srt = Chunks.OrderBy(x => x.Value.Parameters.Ordering);
        var lst = srt.ToList();

        foreach (var chunk in lst)
            data.AddRange(chunk.Value.RawData);

        return [.. data];
    }
}
