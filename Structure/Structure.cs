using System;
using System.IO;
using Game.BlockUtil;
using Game.Core.BlockStorage;
using Game.Core.Chunks;
using Game.Core.Serialization;
using Game.Core.Worlds;
using Game.Util;
using OpenTK.Graphics.Vulkan;
using OpenTK.Mathematics;

namespace Game.Core.Generation;

public class Structure
{

    public ushort[] BlockData;
    public Vector3i Size = Vector3i.Zero;
    public Vector3i Origin = Vector3i.Zero;

    public void OnStructurePlace(World world, Vector3i globalPosition)
    {

        Console.WriteLine(Size);

        for (int x = 0; x < Size.X; x++)
        {

            for (int y = 0; y < Size.Y; y++)
            {

                for (int z = 0; z < Size.Z; z++)
                {

                    Vector3i startingPoint = globalPosition - Origin;

                    Block block = GlobalValues.Register.GetBlockFromId(BlockData[ChunkUtils.VecToIndex((x,y,z))]);

                    if (block != null && block.DisplayName != "Game.Block.AirIgnoreBlock")
                    {

                        Console.WriteLine("hi");
                        block.OnBlockPlace(world, startingPoint + (x,y,z), false, false);

                    }

                }

            }

        }

        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalPosition);
        for (int x = -1; x <= 1; x++)
        {

            for (int y = -1; y <= 1; y++)
            {

                for (int z = -1; z <= 1; z++)
                {

                    world.WorldColumns[chunkPosition.Xz + (x,z)].Chunks[chunkPosition.Y + y].HasUpdates = true;
                    world.WorldColumns[chunkPosition.Xz + (x,z)].QueueType = QueueType.Mesh;
                    WorldGenerator.LowPriorityWorldGenerationQueue.Enqueue(chunkPosition.Xz + (x,z));

                }

            }

        }

    }
    public static Structure LoadFromFile(string file)
    {

        Structure structure = new Structure();

        using (DataReader reader = DataReader.OpenFile(file))
        {

            structure.Size = reader.GetVector3i();
            Vector3i chunkSize = ChunkUtils.PositionToChunk(structure.Size) + Vector3i.One;
            structure.Origin = reader.GetVector3i();
            structure.BlockData = new ushort[(chunkSize.X * GlobalValues.ChunkSize) * (chunkSize.Y * GlobalValues.ChunkSize) * (chunkSize.Z * GlobalValues.ChunkSize)];

            for (int x = 0; x < structure.Size.X; x++)
            {

                for (int y = 0; y < structure.Size.Y; y++)
                {

                    for (int z = 0; z < structure.Size.Z; z++)
                    {

                        structure.BlockData[ChunkUtils.VecToIndex((x,y,z))] = reader.GetUshort();

                    }

                }

            }

        }

        return structure;

    }

    public static void WriteDataToStructFile(World world, Vector3i globalStart, Vector3i globalEnd, Vector3i localOrigin, string fileName)
    {

        Vector3i size = Vector3i.Abs(globalEnd - globalStart);

        Vector3i chunkSize = ChunkUtils.PositionToChunk(size) + Vector3i.One;

        Structure structure = new Structure();
        structure.Size = size;
        structure.Origin = localOrigin;
        structure.BlockData = new ushort[(chunkSize.X * GlobalValues.ChunkSize) * (chunkSize.Y * GlobalValues.ChunkSize) * (chunkSize.Z * GlobalValues.ChunkSize)];

        for (int x = 0; x < size.X; x++)
        {

            for (int y = 0; y < size.Y; y++)
            {

                for (int z = 0; z < size.Z; z++)
                {

                    structure.BlockData[ChunkUtils.VecToIndex((x,y,z))] = world.GetBlockId(globalStart + (x,y,z));

                }

            }

        }

        using (DataWriter writer = DataWriter.OpenFile(fileName))
        {

            writer.WriteVector3i(structure.Size);
            writer.WriteVector3i(structure.Origin);
            for (int x = 0; x < size.X; x++)
            {

                for (int y = 0; y < size.Y; y++)
                {

                    for (int z = 0; z < size.Z; z++)
                    {

                        writer.WriteUshort(structure.BlockData[ChunkUtils.VecToIndex((x,y,z))]);

                    }

                }

            }

        }

        GameLogger.Log($"Finished writing to file {fileName}");

    }

}