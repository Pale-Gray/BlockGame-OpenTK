using Blockgame_OpenTK.ChunkUtil;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Core.World
{
    internal class World
    {

        public string SaveFileName;
        private int MaxRadius = 8;
        public Dictionary<Vector3i, Chunk> WorldChunks = new Dictionary<Vector3i, Chunk>();

        public World(string saveFileName)
        {

            SaveFileName = saveFileName;

            /*
            for (int x = -WorldGenerator.MaxRadius; x <= WorldGenerator.MaxRadius; x++)
            {

                for (int y = -WorldGenerator.MaxRadius; y <= WorldGenerator.MaxRadius; y++)
                {

                    for (int z = -WorldGenerator.MaxRadius; z <= WorldGenerator.MaxRadius; z++)
                    {

                        WorldChunks.Add((x,y,z), new Chunk((x,y,z)));

                    }

                }

            }
            */

        }

        public void DebugReset()
        {

            WorldChunks = new Dictionary<Vector3i, Chunk>();

            WorldGenerator.Reset();

        }

        public void Generate(Vector3i cameraChunkPosition)
        {

            // WorldGenerator.GenerateWorld(this, cameraChunkPosition);

            // WorldGenerator.Generate(this, cameraChunkPosition);

            WorldGenerator.Gen(this, cameraChunkPosition);

        }

        public void Draw(Camera playerCamera)
        {

            if (Globals.ShouldRenderBounds)
            {

                foreach (Vector3i pos in WorldGenerator.ChunksToGeneratePassOne)
                {

                    Game.rmodel.SetScale(32, 32, 32);
                    Game.rmodel.Draw(((Vector3)pos + (0.5f, 0.5f, 0.5f)) * 32, Vector3.Zero, playerCamera, 0);
                    Game.rmodel.SetScale(1, 1, 1);

                }

                foreach (Vector3i pos in WorldGenerator.ChunksToGeneratePassTwo)
                {

                    Game.rmodel.SetScale(32, 32, 32);
                    Game.rmodel.Draw(((Vector3)pos + (0.5f, 0.5f, 0.5f)) * 32, Vector3.Zero, playerCamera, 0);
                    Game.rmodel.SetScale(1, 1, 1);

                }

                foreach (Vector3i pos in WorldGenerator.ChunksToDoMeshPass)
                {

                    Game.rmodel.SetScale(32, 32, 32);
                    Game.rmodel.Draw(((Vector3)pos + (0.5f, 0.5f, 0.5f)) * 32, Vector3.Zero, playerCamera, 0);
                    Game.rmodel.SetScale(1, 1, 1);

                }

            }

            foreach (Chunk chunk in WorldChunks.Values)
            {

                if (chunk.ChunkState == ChunkState.Ready)
                {

                    chunk.Draw((0,1,0), playerCamera);
                    if (Globals.ShouldRenderBounds)
                    {

                        Game.rmodel.SetScale(32, 32, 32);
                        Game.rmodel.Draw(((Vector3)chunk.ChunkPosition + (0.5f, 0.5f, 0.5f)) * 32, Vector3.Zero, playerCamera, 0);
                        Game.rmodel.SetScale(1, 1, 1);

                    }

                }

            }

        }

    }
}
