using System;
using System.Data.SqlTypes;
using System.IO;
using System.Runtime.CompilerServices;
using System.Transactions;
using Blockgame_OpenTK.BlockProperty;
using Blockgame_OpenTK.Core.Serialization;
using Blockgame_OpenTK.Core.Worlds;
using LiteNetLib.Utils;
using OpenTK.Mathematics;
using OpenTK.Platform.Native.X11;

namespace Blockgame_OpenTK.Core.Chunks;

public class ColumnSerializer
{


    public static byte[] SerializeColumnToBytes(ChunkColumn column)
    {

        byte[] arr = Array.Empty<byte>();

        using (DataWriter writer = new DataWriter())
        {

            for (int i = 0; i < PackedWorldGenerator.WorldGenerationHeight; i++)
            {

                Span<byte> span = Compressor.RleCompress<ushort, ushort>(column.Chunks[i].BlockData);

                writer.WriteByteSpan(span);

                for (int s = 0; s < column.Chunks[i].SolidMask.Length; s++)
                {

                    writer.WriteUInt(column.Chunks[i].SolidMask[i]);

                }

            }

            arr = writer.GetUnderlyingBytes();

        }

        Console.WriteLine(arr.Length);

        return arr;

    }

    public static void DeserializeColumnFromBytes(ChunkColumn column, byte[] data)
    {

        using (DataReader reader = new DataReader(data))
        {

            for (int i = 0; i < PackedWorldGenerator.WorldGenerationHeight; i++)
            {

                Span<byte> rle = reader.GetByteSpan();

                column.Chunks[i].BlockData = Compressor.RleDecompress<ushort, ushort>(rle);

                for (int s = 0; s < column.Chunks[i].SolidMask.Length; s++)
                {

                    column.Chunks[i].SolidMask[i] = reader.GetUInt();

                }

            }

        }

    }

    public static void SerializeColumn(ChunkColumn column)
    {

        string fileName = $"{column.Position.X}_{column.Position.Y}.cdat";

        using (DataWriter writer = DataWriter.OpenFile(fileName))
        {

            for (int i = 0; i < PackedWorldGenerator.WorldGenerationHeight; i++)
            {

                Span<byte> span = Compressor.RleCompress<ushort, ushort>(column.Chunks[i].BlockData);

                writer.WriteByteSpan(span);
                
                for (int s = 0; s < column.Chunks[i].SolidMask.Length; s++)
                {

                    writer.WriteUInt(column.Chunks[i].SolidMask[s]);

                }

            }

        }

    }

    public static ChunkColumn DeserializeColumn(ChunkColumn column)
    {

        string fileName = $"{column.Position.X}_{column.Position.Y}.cdat";

        using (DataReader reader = DataReader.OpenFile(fileName))
        {

            for (int i = 0; i < PackedWorldGenerator.WorldGenerationHeight; i++)
            {

                Span<byte> span = reader.GetByteSpan();
                
                column.Chunks[i].BlockData = Compressor.RleDecompress<ushort, ushort>(span);
                for (int s = 0; s < column.Chunks[i].SolidMask.Length; s++)
                {

                    column.Chunks[i].SolidMask[s] = reader.GetUInt();

                }
                // Console.WriteLine($"Current Length: {column.Chunks[i].BlockData.Length}");
                column.Chunks[i].HasUpdates = true;

            }

        }

        return new ChunkColumn(Vector2i.Zero);

    }

}