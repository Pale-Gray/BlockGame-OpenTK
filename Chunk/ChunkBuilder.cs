using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.ChunkUtil
{
    internal class ChunkBuilder
    {
        public static void GenerateThreaded(NewChunk chunk)
        {

            if (chunk.GenerationState == GenerationState.NotGenerated)
            {

                chunk.GenerationState = GenerationState.Generating;
                Task.Run(() => { GeneratePassOne(chunk); });

            }
            if (chunk.GenerationState == GenerationState.PassOne)
            {

                chunk.GenerationState = GenerationState.Generating;
                Task.Run(() => { GeneratePassTwo(chunk); });

            }

        }

        private static void GeneratePassOne(NewChunk chunk)
        {

            for (int x = 0; x < Globals.ChunkSize; x++)
            {

                for (int y = 0; y < Globals.ChunkSize; y++)
                {

                    for (int z = 0; z < Globals.ChunkSize; z++)
                    {

                        chunk.BlockData[x, y, z] = (ushort) Globals.Register.GetIDFromBlock(Blocks.GrassBlock);

                    }

                }

            }
            chunk.GenerationState = GenerationState.PassOne;

        }

        private static void GeneratePassTwo(NewChunk chunk)
        {

            for (int x = 0; x < Globals.ChunkSize; x++)
            {

                for (int y = 0; y < Globals.ChunkSize; y++)
                {

                    for (int z = 0; z < Globals.ChunkSize; z++)
                    {

                        chunk.BlockData[x, y, z] = (ushort)Globals.Register.GetIDFromBlock(Blocks.GrassBlock);

                    }

                }

            }
            chunk.GenerationState = GenerationState.Generated;

        }

    }
}
