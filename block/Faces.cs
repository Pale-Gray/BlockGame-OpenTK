using opentk_proj.util;
using opentk_proj.chunk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj.block
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
