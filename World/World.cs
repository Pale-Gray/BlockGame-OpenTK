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
    public class World
    {

        public string SaveFileName;
        private int MaxRadius = 8;
        private int[] _columnMap = new int[GlobalValues.ChunkSize * GlobalValues.ChunkSize];
        public ConcurrentDictionary<Vector2i, int[]> ColumnMaxBlockHeight = new ConcurrentDictionary<Vector2i, int[]>();
        public ConcurrentDictionary<Vector3i, Chunk> WorldChunks = new ConcurrentDictionary<Vector3i, Chunk>();

        public World()
        {
            
        }
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

                    WorldChunks[playerChunkPosition].GetBlock((randomX, randomY, randomZ)).OnRandomTickUpdate(this, globalBlockPosition, WorldChunks[playerChunkPosition].BlockPropertyData.ContainsKey((randomX, randomY, randomZ)) ? WorldChunks[playerChunkPosition].BlockPropertyData[(randomX, randomY, randomZ)] : null);
                    
                }

            }

        }

        public void AppendModel(BlockModel model, Vector3i globalBlockPosition, Dictionary<Vector3i, Chunk> neighbors)
        {

            Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);
            Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);

            int[] ambientPoints = new int[8];
            // ambientPoints = BlockUtils.CalculateAmbientPoints(mask, globalBlockPosition);

            // Dictionary<Vector3i, bool[]> neighborMasks = ChunkUtils.GetChunkNeighborsSolidMaskDictionary(this, chunkPosition);
            Chunk chunk = WorldChunks[chunkPosition];

            if (!ChunkUtils.GetSolidBlock(neighbors[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitY)], ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitY)))
            {

                if (model.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Up)) chunk.OpaqueMeshList.AddRange(model.GetOffsettedFace(BlockModelCullDirection.Up, localBlockPosition, neighbors));

            }
            if (!ChunkUtils.GetSolidBlock(neighbors[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitY)], ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitY)))
            {

                if (model.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Down)) chunk.OpaqueMeshList.AddRange(model.GetOffsettedFace(BlockModelCullDirection.Down, localBlockPosition, neighbors));

            }
            if (!ChunkUtils.GetSolidBlock(neighbors[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitX)], ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitX)))
            {

                if (model.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Left)) chunk.OpaqueMeshList.AddRange(model.GetOffsettedFace(BlockModelCullDirection.Left, localBlockPosition, neighbors));

            }
            if (!ChunkUtils.GetSolidBlock(neighbors[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitX)], ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitX)))
            {

                if (model.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Right)) chunk.OpaqueMeshList.AddRange(model.GetOffsettedFace(BlockModelCullDirection.Right, localBlockPosition, neighbors));

            }
            if (!ChunkUtils.GetSolidBlock(neighbors[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitZ)], ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitZ)))
            {

                if (model.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Back)) chunk.OpaqueMeshList.AddRange(model.GetOffsettedFace(BlockModelCullDirection.Back, localBlockPosition, neighbors));

            }
            if (!ChunkUtils.GetSolidBlock(neighbors[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitZ)], ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitZ)))
            {

                if (model.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Front)) chunk.OpaqueMeshList.AddRange(model.GetOffsettedFace(BlockModelCullDirection.Front, localBlockPosition, neighbors));

            }
            if (model.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.None)) chunk.OpaqueMeshList.AddRange(model.GetOffsettedFace(BlockModelCullDirection.None, localBlockPosition, neighbors));

        }

        public void AddBlockLight(Vector3i globalBlockPosition, ushort r, ushort g, ushort b)
        {

            Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);

            WorldChunks[chunkPosition].LightAdditionQueue.Enqueue(new ChunkLight(globalBlockPosition, r, g, b));
            // Debugger.Log($"Added light into chunk {chunkPosition}", Severity.Info);

        }

        public void RemoveBlockLight(Vector3i globalBlockPosition, byte r, byte g, byte b)
        {

            WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].LightRemovalQueue.Enqueue(new ChunkLight(globalBlockPosition, r, g, b));

        }

        public void SetBlock(Vector3i globalBlockPosition, IBlockProperties blockProperties, Block block, bool shouldRemesh = true)
        {

            Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);
            Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);

            WorldChunks[chunkPosition].SetBlock(localBlockPosition, blockProperties, block);
            if (ChunkUtils.GetLightValue(WorldChunks[chunkPosition], globalBlockPosition) != ChunkLight.Zero)
            {

                if (block.IsSolid ?? true)
                {

                    ChunkLight removalLight = ChunkUtils.GetLightValue(WorldChunks[chunkPosition], globalBlockPosition);
                    removalLight.LightPosition = globalBlockPosition;
                    WorldChunks[chunkPosition].LightRemovalQueue.Enqueue(removalLight);

                }

            }
            else
            {

                if (block == Blocks.AirBlock)
                {

                    ChunkLight left = ChunkUtils.GetLightValue(
                        WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + Vector3i.UnitX)],
                        globalBlockPosition + Vector3i.UnitX);
                    left.LightPosition = globalBlockPosition + Vector3i.UnitX;
                    if (left != ChunkLight.Zero)
                    {
                        if (!WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + Vector3i.UnitX)].LightAdditionQueue.Contains(left)) WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + Vector3i.UnitX)].LightAdditionQueue.Enqueue(left);
                    }
                    
                    ChunkLight right = ChunkUtils.GetLightValue(
                        WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition - Vector3i.UnitX)],
                        globalBlockPosition - Vector3i.UnitX);
                    right.LightPosition = globalBlockPosition - Vector3i.UnitX;
                    if (right != ChunkLight.Zero)
                    {
                        if (!WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition - Vector3i.UnitX)].LightAdditionQueue.Contains(right)) WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition - Vector3i.UnitX)].LightAdditionQueue.Enqueue(right);
                    }
                    
                    ChunkLight front = ChunkUtils.GetLightValue(
                        WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition - Vector3i.UnitZ)],
                        globalBlockPosition - Vector3i.UnitZ);
                    front.LightPosition = globalBlockPosition - Vector3i.UnitZ;
                    if (front != ChunkLight.Zero)
                    {
                        if (!WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition - Vector3i.UnitZ)].LightAdditionQueue.Contains(front)) WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition - Vector3i.UnitZ)].LightAdditionQueue.Enqueue(front);
                    }
                    
                    ChunkLight back = ChunkUtils.GetLightValue(
                        WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + Vector3i.UnitZ)],
                        globalBlockPosition + Vector3i.UnitZ);
                    back.LightPosition = globalBlockPosition + Vector3i.UnitZ;
                    if (back != ChunkLight.Zero)
                    {
                        if (!WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + Vector3i.UnitZ)].LightAdditionQueue.Contains(back)) WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + Vector3i.UnitZ)].LightAdditionQueue.Enqueue(back);
                    }
                    
                    ChunkLight up = ChunkUtils.GetLightValue(
                        WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + Vector3i.UnitY)],
                        globalBlockPosition + Vector3i.UnitY);
                    up.LightPosition = globalBlockPosition + Vector3i.UnitY;
                    if (up != ChunkLight.Zero)
                    {
                        if (!WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + Vector3i.UnitY)].LightAdditionQueue.Contains(up)) WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + Vector3i.UnitY)].LightAdditionQueue.Enqueue(up);
                    }
                    
                    ChunkLight down = ChunkUtils.GetLightValue(
                        WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition - Vector3i.UnitY)],
                        globalBlockPosition - Vector3i.UnitY);
                    down.LightPosition = globalBlockPosition - Vector3i.UnitY;
                    if (down != ChunkLight.Zero)
                    {
                        if (!WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition - Vector3i.UnitY)].LightAdditionQueue.Contains(down)) WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition - Vector3i.UnitY)].LightAdditionQueue.Enqueue(down);
                    }

                }
                
            }

            if (shouldRemesh)
            {

                WorldChunks[chunkPosition].QueueType = QueueType.LightPropagation;
                WorldGenerator.ConcurrentChunkUpdateQueue.Enqueue(chunkPosition);

                for (int x = -1; x <= 1; x++)
                {

                    for (int y = -1; y <= 1; y++)
                    {

                        for (int z = -1; z <= 1; z++)
                        {

                            if ((x, y, z) != Vector3i.Zero)
                            {

                                WorldChunks[chunkPosition + (x, y, z)].QueueType = QueueType.LightPropagation;
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
            if (WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockPropertyData.TryGetValue(ChunkUtils.PositionToBlockLocal(globalBlockPosition), out IBlockProperties prop))
            {
                return prop;
            }
            return null;
            // return WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].BlockPropertyData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition))];

        }

    }
}
