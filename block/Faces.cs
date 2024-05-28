using Blockgame_OpenTK.ChunkUtil;

namespace Blockgame_OpenTK.BlockUtil
{
    internal class Faces
    {

        public static ChunkVertex[] FrontFace =
        {

            new ChunkVertex(0, 1, 1, 0, 0, 1, 0, 0, -1), 
            new ChunkVertex(0, 1, 0, 0, 0, 0, 0, 0, -1),
            new ChunkVertex(0, 0, 0, 0, 1, 0, 0, 0, -1),
            new ChunkVertex(0, 0, 0, 0, 1, 0, 0, 0, -1),
            new ChunkVertex(0, 0, 1, 0, 1, 1, 0, 0, -1),
            new ChunkVertex(0, 1, 1, 0, 0, 1, 0, 0, -1)

        };
        public static ChunkVertex[] RightFace =
        {

            new ChunkVertex(0, 0, 1, 0, 0, 0, -1, 0, 0),
            new ChunkVertex(0, 0, 0, 0, 0, 0, -1, 0, 0),
            new ChunkVertex(0, 0, 0, 1, 0, 0, -1, 0, 0),
            new ChunkVertex(0, 0, 0, 1, 0, 0, -1, 0, 0),
            new ChunkVertex(0, 0, 1, 1, 0, 0, -1, 0, 0),
            new ChunkVertex(0, 0, 1, 0, 0, 0, -1, 0, 0)

        };
        public static ChunkVertex[] BackFace =
        {

            new ChunkVertex(0, 0, 1, 1, 0, 0, 0, 0, 1),
            new ChunkVertex(0, 0, 0, 1, 0, 0, 0, 0, 1),
            new ChunkVertex(0, 1, 0, 1, 0, 0, 0, 0, 1),
            new ChunkVertex(0, 1, 0, 1, 0, 0, 0, 0, 1),
            new ChunkVertex(0, 1, 1, 1, 0, 0, 0, 0, 1),
            new ChunkVertex(0, 0, 1, 1, 0, 0, 0, 0, 1)

        };
        public static ChunkVertex[] LeftFace =
        {

            new ChunkVertex(0, 1, 1, 1, 0, 0, 1, 0, 0),
            new ChunkVertex(0, 1, 0, 1, 0, 0, 1, 0, 0),
            new ChunkVertex(0, 1, 0, 0, 0, 0, 1, 0, 0),
            new ChunkVertex(0, 1, 0, 0, 0, 0, 1, 0, 0),
            new ChunkVertex(0, 1, 1, 0, 0, 0, 1, 0, 0),
            new ChunkVertex(0, 1, 1, 1, 0, 0, 1, 0, 0)

        };
        public static ChunkVertex[] TopFace =
        {

            new ChunkVertex(0, 1, 1, 1, 0, 0, 0, 1, 0),
            new ChunkVertex(0, 1, 1, 0, 0, 0, 0, 1, 0),
            new ChunkVertex(0, 0, 1, 0, 0, 0, 0, 1, 0),
            new ChunkVertex(0, 0, 1, 0, 0, 0, 0, 1, 0),
            new ChunkVertex(0, 0, 1, 1, 0, 0, 0, 1, 0),
            new ChunkVertex(0, 1, 1, 1, 0, 0, 0, 1, 0)

        };
        public static ChunkVertex[] BottomFace =
        {

            new ChunkVertex(0, 0, 0, 1, 0, 0, 0, -1, 0),
            new ChunkVertex(0, 0, 0, 0, 0, 0, 0, -1, 0),
            new ChunkVertex(0, 1, 0, 0, 0, 0, 0, -1, 0),
            new ChunkVertex(0, 1, 0, 0, 0, 0, 0, -1, 0),
            new ChunkVertex(0, 1, 0, 1, 0, 0, 0, -1, 0),
            new ChunkVertex(0, 0, 0, 1, 0, 0, 0, -1, 0)

        };

    }
}
