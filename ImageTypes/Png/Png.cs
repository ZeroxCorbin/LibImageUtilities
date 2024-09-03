using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibImageUtilities.ImageTypes.Png;
public class Png
{
    public Signature Signature { get; set; } = new();
    public List<IChunk> Chunks { get; set; } = [];

    public static Dictionary<ChunkTypes, IChunk> GetPngChunks(byte[] png)
    {
        Dictionary<ChunkTypes, IChunk> chunks = new();

        using MemoryStream ms = new(png);
        using BinaryReader reader = new(ms);

        // Skip the PNG signature
        reader.BaseStream.Seek(8, SeekOrigin.Begin);

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            int chunkLength = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
            int chunkType = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);

            if (Enum.IsDefined(typeof(ChunkTypes), chunkType))
            {
                if (!chunks.ContainsKey((ChunkTypes)chunkType))
                    chunks.Add((ChunkTypes)chunkType, GetChunkData(reader, chunkLength, (ChunkTypes)chunkType));
                else
                {
                    chunks[(ChunkTypes)chunkType].Data.AddRange(reader.ReadBytes(chunkLength));
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                }
            }
            else
                reader.BaseStream.Seek(4 + chunkLength, SeekOrigin.Current);
        }

        return chunks;
    }
    public static IChunk GetChunkData(BinaryReader reader, int chunkLength, ChunkTypes chunkType)
    {
        List<byte> data = new();

        for (int i = 0; i < chunkLength + 4; i++)
            data.Add(reader.ReadByte());

        switch (chunkType)
        {
            case ChunkTypes.IHDR:
                return new IHDR_Chunk(data);
            case ChunkTypes.PLTE:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.IDAT:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.IEND:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.pHYs:
                return new PHYS_Chunk(data);
            case ChunkTypes.tEXt:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.zTXt:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.tIME:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.tRNS:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.cHRM:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.gAMA:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.iCCP:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.sBIT:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.sRGB:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.iTXt:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.bKGD:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.hIST:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.pCAL:
                return new Generic_Chunk(data, chunkType);
            case ChunkTypes.sPLT:
                return new Generic_Chunk(data, chunkType);

        }

        return null;
    }
}
