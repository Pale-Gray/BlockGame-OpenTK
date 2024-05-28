using System.Collections.Generic;

namespace Blockgame_OpenTK.BlockUtil
{
    internal class Blocks
    {

        public static List<Block> BlockList = new List<Block>();

        public static Block Air = new Block("Air").SetFaceTexture(1, 0, 0).Register();

        public static Block Grass = new Block("Grass").SetFaceTexture(1, 0, 0, 1, 2, 3).SetFaceTexture(0, 0, 5).SetFaceTexture(2, 0, 4).Register();
        public static Block Dirt = new Block("Dirt").SetFaceTexture(0, 0, 0, 1, 2, 3, 4, 5).Register();
        public static Block Stone = new Block("Stone").SetFaceTexture(3, 0, 0, 1, 2, 3, 4, 5).Register();

        public static int GetIDFromBlock(Block block)
        {

            return BlockList.IndexOf(block);

        }

        public static Block GetBlockFromID(int index)
        {

            return BlockList[index];

        }

    }
}
