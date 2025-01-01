using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System.Collections.Generic;

using Blockgame_OpenTK.Core.Chunks;
using OpenTK.Graphics.OpenGL;
using System;
using System.Security.Cryptography;
using Blockgame_OpenTK.PlayerUtil;
using Blockgame_OpenTK.BlockUtil;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Collections.Concurrent;
using System.Linq;
using Blockgame_OpenTK.BlockProperty;

namespace Blockgame_OpenTK.Core.Worlds
{
    internal class World
    {

        public string SaveFileName;
        private int MaxRadius = 8;
        private int[] _columnMap = new int[GlobalValues.ChunkSize * GlobalValues.ChunkSize];
        public ConcurrentDictionary<Vector2i, int[]> ColumnMaxBlockHeight = new ConcurrentDictionary<Vector2i, int[]>();
        public ConcurrentDictionary<Vector3i, Chunk> WorldChunks = new ConcurrentDictionary<Vector3i, Chunk>();

        public World(string saveFileName)
        {

            SaveFileName = saveFileName;

        }

        public void DebugReset()
        {

            WorldGenerator.Reset();

            WorldChunks.Clear();

        }
        public void Generate(Vector3 playerPosition)
        {

            WorldGenerator.GenerateWorld(this, playerPosition);

        }

        public void Draw(Camera playerCamera)
        {

            foreach (KeyValuePair<Vector3i, Chunk> pair in WorldChunks)
            {

                if (pair.Value.IsRenderable && pair.Value.MeshIndices.Length != 0)
                {

                    // GL.Disable(EnableCap.CullFace);
                    pair.Value.Draw((0, -1, 0), playerCamera);
                    // GL.Enable(EnableCap.CullFace);

                }

            }

        }

        public void Tick(Vector3 playerPosition)
        {

            Vector3i playerChunkPosition = ChunkUtils.PositionToChunk(playerPosition);

            if (WorldChunks.ContainsKey(playerChunkPosition))
            {

                for (int i = 0; i < 1000; i++)
                {

                    Random r = new Random();
                    int randomX = r.Next(0, 32);
                    int randomY = r.Next(0, 32);
                    int randomZ = r.Next(0, 32);

                    Vector3i globalBlockPosition = (randomX, randomY, randomZ) + (playerChunkPosition * 32);

                    WorldChunks[playerChunkPosition].GetBlock((randomX, randomY, randomZ)).OnRandomTickUpdate(this, globalBlockPosition, WorldChunks[playerChunkPosition].BlockPropertyData[ChunkUtils.VecToIndex((randomX, randomY, randomZ))]);

                }

            }

        }

        public void AppendModel(BlockModel model, Vector3i globalBlockPosition, Dictionary<Vector3i, bool[]> mask)
        {

            Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);
            Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);

            int[] ambientPoints = new int[8];
            // ambientPoints = BlockUtils.CalculateAmbientPoints(mask, globalBlockPosition);

            // Dictionary<Vector3i, bool[]> neighborMasks = ChunkUtils.GetChunkNeighborsSolidMaskDictionary(this, chunkPosition);
            Chunk chunk = WorldChunks[chunkPosition];

            if (!mask[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitY)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitY))])
            {

                if (model.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Up)) chunk.OpaqueMeshList.AddRange(model.GetOffsettedFace(BlockModelCullDirection.Up, localBlockPosition, mask));

            }
            if (!mask[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitY)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitY))])
            {

                if (model.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Down)) chunk.OpaqueMeshList.AddRange(model.GetOffsettedFace(BlockModelCullDirection.Down, localBlockPosition, mask));

            }
            if (!mask[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitX)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitX))])
            {

                if (model.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Left)) chunk.OpaqueMeshList.AddRange(model.GetOffsettedFace(BlockModelCullDirection.Left, localBlockPosition, mask));

            }
            if (!mask[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitX)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitX))])
            {

                if (model.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Right)) chunk.OpaqueMeshList.AddRange(model.GetOffsettedFace(BlockModelCullDirection.Right, localBlockPosition, mask));

            }
            if (!mask[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitZ)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitZ))])
            {

                if (model.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Back)) chunk.OpaqueMeshList.AddRange(model.GetOffsettedFace(BlockModelCullDirection.Back, localBlockPosition, mask));

            }
            if (!mask[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitZ)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitZ))])
            {

                if (model.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Front)) chunk.OpaqueMeshList.AddRange(model.GetOffsettedFace(BlockModelCullDirection.Front, localBlockPosition, mask));

            }
            if (model.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.None)) chunk.OpaqueMeshList.AddRange(model.GetOffsettedFace(BlockModelCullDirection.None, localBlockPosition, mask));

        }

        public void AddBlockLight(Vector3i globalBlockPosition, Vector3i color)
        {

            Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);
            Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);

            WorldChunks[chunkPosition].GlobalBlockLightPositions.Add(globalBlockPosition, color);

        }

        public void RemoveBlockLight(Vector3i globalBlockPosition)
        {

            Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);
            WorldChunks[chunkPosition].GlobalBlockLightPositions.Remove(globalBlockPosition);

        }

        public void SetBlock(Vector3i globalBlockPosition, IBlockProperties blockProperties, Block block, bool shouldRemesh = true)
        {

            Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);
            Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);

            WorldChunks[chunkPosition].SetBlock(localBlockPosition, blockProperties, block);

            if (shouldRemesh)
            {

                WorldChunks[chunkPosition].QueueType = QueueType.Mesh;
                WorldGenerator.ConcurrentChunkUpdateQueue.Enqueue(chunkPosition);

                for (int x = -1; x <= 1; x++)
                {

                    for (int y = -1; y <= 1; y++)
                    {

                        for (int z = -1; z <= 1; z++)
                        {

                            if ((x, y, z) != Vector3i.Zero)
                            {

                                WorldChunks[chunkPosition + (x, y, z)].QueueType = QueueType.Mesh;
                                WorldGenerator.ConcurrentChunkUpdateQueue.Enqueue(chunkPosition + (x, y, z));

                            }

                        }

                    }

                }

            }

        }

        public Block GetBlock(Vector3i globalBlockPosition)
        {

            return WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].GetBlock(ChunkUtils.PositionToBlockLocal(globalBlockPosition));

        }

        public IBlockProperties GetBlockProperties(Vector3i globalBlockPosition)
        {

            // return WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockPropertyData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition))];

            return WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockPropertyData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition))];

        }

    }
}
