using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibImageUtilities.ImageTypes.Png;
public class Png(byte[] image)
{
    public const int IntSize = 4;

    public Signature Signature { get; set; } = new();
    public Dictionary<ChunkTypes, IChunk> Chunks { get; set; } = GetPngChunks(image);

    public static Dictionary<ChunkTypes, IChunk> GetPngChunks(byte[] png)
    {
        Dictionary<ChunkTypes, IChunk> chunks = [];

        using MemoryStream ms = new(png);
        using BinaryReader reader = new(ms);

        // Skip the PNG signature
        reader.BaseStream.Seek(Signature.Length, SeekOrigin.Begin);

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            int chunkLength = BitConverter.ToInt32(reader.ReadBytes(IntSize).Reverse().ToArray(), 0);
            int chunkType = BitConverter.ToInt32(reader.ReadBytes(IntSize).Reverse().ToArray(), 0);

            if (Enum.IsDefined(typeof(ChunkTypes), chunkType))
            {
                if (!chunks.ContainsKey((ChunkTypes)chunkType))
                    chunks.Add((ChunkTypes)chunkType, GetChunkData(reader, chunkLength, (ChunkTypes)chunkType));
                else
                {
                    chunks[(ChunkTypes)chunkType].Data.AddRange(reader.ReadBytes(chunkLength));
                    reader.BaseStream.Seek(IChunk.CrcSize, SeekOrigin.Current);
                }
            }
            else
                reader.BaseStream.Seek(IChunk.CrcSize + chunkLength, SeekOrigin.Current);
        }

        return chunks;
    }
    private static IChunk GetChunkData(BinaryReader reader, int chunkLength, ChunkTypes chunkType)
    {
        List<byte> data = [];

        for (int i = 0; i < chunkLength + IChunk.CrcSize; i++)
            data.Add(reader.ReadByte());

        return chunkType switch
        {
            ChunkTypes.IHDR => new IHDR_Chunk(data),
            ChunkTypes.PLTE => new Generic_Chunk(data, chunkType),
            ChunkTypes.IDAT => new Generic_Chunk(data, chunkType),
            ChunkTypes.IEND => new Generic_Chunk(data, chunkType),
            ChunkTypes.pHYs => new PHYS_Chunk(data),
            ChunkTypes.tEXt => new Generic_Chunk(data, chunkType),
            ChunkTypes.zTXt => new Generic_Chunk(data, chunkType),
            ChunkTypes.tIME => new Generic_Chunk(data, chunkType),
            ChunkTypes.tRNS => new Generic_Chunk(data, chunkType),
            ChunkTypes.cHRM => new Generic_Chunk(data, chunkType),
            ChunkTypes.gAMA => new Generic_Chunk(data, chunkType),
            ChunkTypes.iCCP => new Generic_Chunk(data, chunkType),
            ChunkTypes.sBIT => new Generic_Chunk(data, chunkType),
            ChunkTypes.sRGB => new Generic_Chunk(data, chunkType),
            ChunkTypes.iTXt => new Generic_Chunk(data, chunkType),
            ChunkTypes.bKGD => new Generic_Chunk(data, chunkType),
            ChunkTypes.hIST => new Generic_Chunk(data, chunkType),
            ChunkTypes.pCAL => new Generic_Chunk(data, chunkType),
            ChunkTypes.sPLT => new Generic_Chunk(data, chunkType),
            _ => new Generic_Chunk(data, chunkType),
        };
    }
}
