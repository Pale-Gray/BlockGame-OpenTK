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
        public static Block Grass = new Block().SetID(1).SetFront(1, 0).SetRight(1, 0).SetBack(1, 0).SetLeft(1, 0).SetTop(2, 0).SetBottom(0, 0);
        public static Block Dirt = new Block().SetID(2).SetFront(0, 0).SetRight(0, 0).SetBack(0, 0).SetLeft(0, 0).SetTop(0, 0).SetBottom(0, 0);
        public static Block Stone = new Block().SetID(3).SetFront(3, 0).SetRight(3, 0).SetBack(3, 0).SetLeft(3, 0).SetTop(3, 0).SetBottom(3, 0);

        public static Block PRIDERAHHHH = new Block().SetID(99).SetFront(3, 2).SetRight(3, 2).SetBack(3, 2).SetLeft(3, 2).SetTop(3, 2).SetBottom(3, 2);

        public static Dictionary<int, Block> BlockIDs = new Dictionary<int, Block>();

        public static void RegisterIDs()
        {

            BlockIDs[Air.GetID()] = Air;
            BlockIDs[Grass.GetID()] = Grass;
            BlockIDs[Dirt.GetID()] = Dirt;
            BlockIDs[Stone.GetID()] = Stone;

            BlockIDs[PRIDERAHHHH.GetID()] = PRIDERAHHHH;

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
