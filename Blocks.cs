using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj
{
    internal class Blocks : Block
    {

        public static Block Air = new Block();
        public static GrassBlock Grass = new GrassBlock();

        public static Dictionary<int, Block> BlockIDs = new Dictionary<int, Block>();

        public static void RegisterIDs()
        {

            BlockIDs[Air.GetID()] = Air;
            BlockIDs[Grass.GetID()] = Grass;

        }

        public static Block GetBlockByID(int id)
        {

            return BlockIDs[id];

        } 

    }
}
