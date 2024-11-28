using Blockgame_OpenTK.Core.Chunks;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.BlockUtil
{
    internal class BlockUtils
    {

        public static int[] CalculateAmbientPoints(Dictionary<Vector3i, bool[]> masks, Vector3i globalBlockPositon)
        {

            int[] ambientPoints = new int[8];

            Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPositon);
            // Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPositon);

            /*
             * 
             * AO INDEX VISUALIZATION
             * 
             *               +Y (UP)
             *                |
             * +X (LEFT) _____|
             *                \
             *                 \
             *                 -Z (FRONT)
             *       
             *  0------------2
             *  \            |\
             *  |\           | \
             *  | \          |  \
             *  |  \         |   \
             *  |   1------------3
             *  |   |        |   |
             *  4---|--------6   |
             *  \   |         \  |
             *   \  |          \ |
             *    \ |           \|
             *     \|            |
             *      5------------7
             * 
             */

            // TOP
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitX + Vector3i.UnitZ + Vector3i.UnitY)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitX + Vector3i.UnitZ + Vector3i.UnitY))])
            {

                ambientPoints[0]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitX + Vector3i.UnitY)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitX + Vector3i.UnitY))])
            {

                ambientPoints[0]++;
                ambientPoints[1]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitX + Vector3i.UnitY - Vector3i.UnitZ)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitX + Vector3i.UnitY - Vector3i.UnitZ))])
            {

                ambientPoints[1]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitZ + Vector3i.UnitY)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitZ + Vector3i.UnitY))])
            {

                ambientPoints[0]++;
                ambientPoints[2]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitZ + Vector3i.UnitY)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitZ + Vector3i.UnitY))])
            {

                ambientPoints[1]++;
                ambientPoints[3]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitX + Vector3i.UnitZ + Vector3i.UnitY)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitX + Vector3i.UnitZ + Vector3i.UnitY))])
            {

                ambientPoints[2]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitX + Vector3i.UnitY)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitX + Vector3i.UnitY))])
            {

                ambientPoints[2]++;
                ambientPoints[3]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitX + Vector3i.UnitY - Vector3i.UnitZ)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitX + Vector3i.UnitY - Vector3i.UnitZ))])
            {

                ambientPoints[3]++;

            }

            // MIDDLE
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitX + Vector3i.UnitZ)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitX + Vector3i.UnitZ))])
            {

                ambientPoints[0]++;
                ambientPoints[4]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitX - Vector3i.UnitZ)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitX - Vector3i.UnitZ))])
            {

                ambientPoints[1]++;
                ambientPoints[5]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitX + Vector3i.UnitZ)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitX + Vector3i.UnitZ))])
            {

                ambientPoints[2]++;
                ambientPoints[6]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitX - Vector3i.UnitZ)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitX - Vector3i.UnitZ))])
            {

                ambientPoints[3]++;
                ambientPoints[7]++;

            }
            
            // BOTTOM
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitX + Vector3i.UnitZ - Vector3i.UnitY)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitX + Vector3i.UnitZ + Vector3i.UnitY))])
            {

                ambientPoints[4]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitX - Vector3i.UnitY)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitX + Vector3i.UnitY))])
            {

                ambientPoints[4]++;
                ambientPoints[5]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitX - Vector3i.UnitY - Vector3i.UnitZ)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitX + Vector3i.UnitY - Vector3i.UnitZ))])
            {

                ambientPoints[5]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition + Vector3i.UnitZ - Vector3i.UnitY)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition + Vector3i.UnitZ + Vector3i.UnitY))])
            {

                ambientPoints[4]++;
                ambientPoints[6]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitZ - Vector3i.UnitY)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitZ + Vector3i.UnitY))])
            {

                ambientPoints[5]++;
                ambientPoints[7]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitX + Vector3i.UnitZ - Vector3i.UnitY)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitX + Vector3i.UnitZ + Vector3i.UnitY))])
            {

                ambientPoints[6]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitX - Vector3i.UnitY)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitX + Vector3i.UnitY))])
            {

                ambientPoints[6]++;
                ambientPoints[7]++;

            }
            if (masks[ChunkUtils.PositionToChunk(localBlockPosition - Vector3i.UnitX - Vector3i.UnitY - Vector3i.UnitZ)][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localBlockPosition - Vector3i.UnitX + Vector3i.UnitY - Vector3i.UnitZ))])
            {

                ambientPoints[7]++;

            }

            return ambientPoints;

        }

    }
}
