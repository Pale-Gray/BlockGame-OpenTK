using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Blockgame_OpenTK.BlockUtil
{

    public struct BlockVertex
    {

        Vector3 Position;
        int TextureIndex;

        public BlockVertex(Vector3 position, int textureIndex)
        {

            Position = position;
            TextureIndex = textureIndex;

        }

    }
    internal class BlockModel
    {

        List<BlockVertex> Vertices;

    }

}
