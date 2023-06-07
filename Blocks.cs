using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using opentk_proj.blocks;

namespace opentk_proj
{
    internal class Blocks : Block
    {

        public static Block Air = new Block();
        public static GrassBlock Grass = new GrassBlock();
        public static DirtBlock Dirt = new DirtBlock();

        public static Dictionary<int, Block> BlockIDs = new Dictionary<int, Block>();

        public static void RegisterIDs()
        {

            BlockIDs[Air.GetID()] = Air;
            BlockIDs[Grass.GetID()] = Grass;
            BlockIDs[Dirt.GetID()] = Dirt;

            // BlockIDs[Dirt.GetID()] = Dirt;

        }

        public static Block GetBlockByID(int id)
        {

            return BlockIDs[id];

        } 

        public static int GetIDByBlock(Block block)
        {

            return block.GetID();

        }

    }
}
