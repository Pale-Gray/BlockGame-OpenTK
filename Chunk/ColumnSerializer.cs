using System;
using System.Runtime.CompilerServices;
using System.Transactions;
using Blockgame_OpenTK.BlockProperty;
using Blockgame_OpenTK.Core.Serialization;
using Blockgame_OpenTK.Core.Worlds;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Chunks;

public class ColumnSerializer
{

    public static void SerializeColumn(ChunkColumn column)
    {

        string fileName = $"{column.Position.X}_{column.Position.Y}.cdat";

        using (DataWriter writer = DataWriter.Open(fileName))
        {

            for (int i = 0; i < PackedWorldGenerator.WorldGenerationHeight; i++)
            {

                Span<byte> span = Compressor.RleCompress<ushort, ushort>(column.Chunks[i].BlockData);

                // Console.WriteLine(span.Length);
                writer.WriteByteSpan(span);

            }

        }

    }

    public static ChunkColumn DeserializeColumn(ChunkColumn column)
    {

        string fileName = $"{column.Position.X}_{column.Position.Y}.cdat";

        using (DataReader reader = DataReader.Open(fileName))
        {

            for (int i = 0; i < PackedWorldGenerator.WorldGenerationHeight; i++)
            {

                Span<byte> span = reader.GetByteSpan();
                // Console.WriteLine($"Span length: {span.Length}");
                // Console.WriteLine($"Previous length: {column.Chunks[i].BlockData.Length}");
                column.Chunks[i].BlockData = Compressor.RleDecompress<ushort, ushort>(span);
                // Console.WriteLine($"Current Length: {column.Chunks[i].BlockData.Length}");
                column.Chunks[i].HasUpdates = true;

            }

        }

        return new ChunkColumn(Vector2i.Zero);

    }

}