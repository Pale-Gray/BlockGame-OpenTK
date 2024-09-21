using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System.Collections.Generic;

using Blockgame_OpenTK.Core.Chunks;
using OpenTK.Graphics.OpenGL;
using System;

namespace Blockgame_OpenTK.Core.Worlds
{
    internal class World
    {

        public string SaveFileName;
        private int MaxRadius = 8;
        public Dictionary<Vector3i, Chunks.Chunk> WorldChunks = new Dictionary<Vector3i, Chunks.Chunk>();

        public World(string saveFileName)
        {

            SaveFileName = saveFileName;

        }

        public void DebugReset()
        {

            WorldGenerator.Reset();

            WorldChunks.Clear();

        }

        public void Generate(Vector3 playerPositiom)
        {

            WorldGenerator.GenerateWorld(this, playerPositiom);

        }

        public void Draw(Camera playerCamera)
        {

            foreach (Chunk chunk in WorldChunks.Values)
            {

                if (chunk.QueueType == QueueType.Finish && chunk.ChunkMesh.Length != 0)
                {

                    GL.PolygonMode(TriangleFace.FrontAndBack, GlobalValues.ShouldRenderWireframe ? PolygonMode.Line : PolygonMode.Fill);
                    chunk.Draw((0,1,0), playerCamera);
                    GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

                    if (GlobalValues.ShouldRenderBounds)
                    {

                        // Game.rmodel.SetScale(32, 32, 32);
                        // Game.rmodel.Draw(((Vector3)chunk.ChunkPosition + (0.5f, 0.5f, 0.5f)) * 32, Vector3.Zero, playerCamera, 0);
                        // Game.rmodel.SetScale(1, 1, 1);

                    }

                }

            }

        }

    }
}
