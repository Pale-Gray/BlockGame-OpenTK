using System;
using System.Data.SqlTypes;
using System.IO;
using System.Runtime.CompilerServices;
using System.Transactions;
using Game.Core.BlockStorage;
using Game.Core.Serialization;
using Game.Core.Worlds;
using Game.Util;
using LiteNetLib.Utils;
using OpenTK.Mathematics;
using OpenTK.Platform.Native.X11;

namespace Game.Core.Chunks;

public class ColumnSerializer
{

    public static void SerializeColumn(ChunkColumn column)
    {

        using (DataWriter writer = DataWriter.OpenFile(Path.Combine("Worlds", GameState.World.WorldPath, "Chunks", ColumnUtils.PositionToFilename(column.Position))))
        {

            for (int y = WorldGenerator.WorldGenerationHeight - 1; y >= 0; y--)
            {

                writer.WriteByteSpan(Compressor.RleCompress<ushort, ushort>(column.Chunks[y].BlockData));
                writer.WriteByteSpan(Compressor.RleCompress<ushort, ushort>(column.Chunks[y].LightData));

                for (int i = 0; i < column.Chunks[y].SolidMask.Length; i++)
                {

                    writer.WriteUInt(column.Chunks[y].SolidMask[i]);

                }

            }

        }

    }

    public static bool TryDeserializeColumn(ChunkColumn column)
    {

        if (File.Exists(Path.Combine("Worlds", GameState.World.WorldPath, "Chunks", ColumnUtils.PositionToFilename(column.Position))))
        {

            DeserializeColumn(column);
            return true;

        }
        return false;

    }
    public static void DeserializeColumn(ChunkColumn column)
    {

        using (DataReader reader = DataReader.OpenFile(Path.Combine("Worlds", GameState.World.WorldPath, "Chunks", ColumnUtils.PositionToFilename(column.Position))))
        {

            for (int y = WorldGenerator.WorldGenerationHeight - 1; y >= 0; y--)
            {

                column.Chunks[y].BlockData = Compressor.RleDecompress<ushort, ushort>(reader.GetByteSpan());
                column.Chunks[y].LightData = Compressor.RleDecompress<ushort, ushort>(reader.GetByteSpan());

                for (int i = 0; i < column.Chunks[y].SolidMask.Length; i++)
                {

                    column.Chunks[y].SolidMask[i] = reader.GetUInt();

                }

                column.Chunks[y].HasUpdates = true;

            }

        }

    }
    public static byte[] SerializeColumnToBytes(ChunkColumn column)
    {

        byte[] arr = Array.Empty<byte>();

        using (DataWriter writer = new DataWriter())
        {

            for (int i = 0; i < WorldGenerator.WorldGenerationHeight; i++)
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

            for (int i = 0; i < WorldGenerator.WorldGenerationHeight; i++)
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

    /*
    public static void SerializeColumn(ChunkColumn column)
    {

        string fileName = $"{column.Position.X}_{column.Position.Y}.cdat";

        if (!Directory.Exists("Worlds")) Directory.CreateDirectory("Worlds");

        if (!Directory.Exists(Path.Combine("Worlds", GameState.World.WorldPath))) Directory.CreateDirectory(Path.Combine("Worlds", GameState.World.WorldPath));

        using (DataWriter writer = DataWriter.OpenFile(Path.Combine("Worlds", GameState.World.WorldPath, fileName)))
        {

            for (int i = 0; i < WorldGenerator.WorldGenerationHeight; i++)
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
    */

    /*

    public static ChunkColumn DeserializeColumn(ChunkColumn column)
    {

        string fileName = $"{column.Position.X}_{column.Position.Y}.cdat";

        using (DataReader reader = DataReader.OpenFile(fileName))
        {

            for (int i = 0; i < WorldGenerator.WorldGenerationHeight; i++)
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
    */

}