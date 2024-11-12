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

namespace Blockgame_OpenTK.Core.Worlds
{
    internal class World
    {

        public string SaveFileName;
        private int MaxRadius = 8;
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

            foreach (Chunk chunk in WorldChunks.Values)
            {

                if (chunk.IsRenderable && chunk.MeshIndices.Length != 0)
                {

                    GL.Disable(EnableCap.CullFace);
                    chunk.Draw((0,-1,0), playerCamera);
                    GL.Enable(EnableCap.CullFace);

                }

            }

        }


        public void DrawRadius(Player player, int radius)
        {

            /*
            Vector3i playerChunkPosition = ChunkUtils.PositionToChunk(player.Position);
            foreach (Chunk chunk in WorldChunks.Values)
            {

                if (chunk.QueueType == QueueType.Done && chunk.SolidMesh.Length != 0 && Maths.ChebyshevDistance3D(playerChunkPosition, chunk.ChunkPosition) <= radius)
                {

                    chunk.Draw((0, 1, 0), player.Camera);

                }

            }
            */

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

        public void SetBlock(Vector3i globalBlockPosition, BlockProperties blockProperties, ushort blockId)
        {

            Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);
            Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);

            WorldChunks[chunkPosition].SetBlock(localBlockPosition, GlobalValues.Register.GetBlockFromID(blockId));
            WorldChunks[chunkPosition].QueueType = QueueType.LightPropagation;
            WorldGenerator.ConcurrentChunkUpdateQueue.Enqueue(chunkPosition);

            Vector3i componentMin = Vector3i.ComponentMin(localBlockPosition, (-1, -1, -1));
            Vector3i componentMax = Vector3i.ComponentMax(localBlockPosition, Vector3i.Zero);

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

        public Block GetBlock(Vector3i globalBlockPosition)
        {

            return WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].GetBlock(ChunkUtils.PositionToBlockLocal(globalBlockPosition));

        }

    }
}
