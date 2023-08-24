using opentk_proj.registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj.block
{
    internal class Blocks
    {

        public static Block Air = new Block();
        public static Block Grass = new Block().SetID(1).SetFaces(1, 0, Block.SIDES).SetFaces(2,0,Block.TOF).SetFaces(0,0,Block.BOF);
        public static Block Dirt = new Block().SetID(2).SetFaces(0, 0, Block.ALL);
        public static Block Stone = new Block().SetID(3).SetFront(3, 0).SetRight(3, 0).SetBack(3, 0).SetLeft(3, 0).SetTop(3, 0).SetBottom(3, 0);
        public static Block Maple_Log = new Block().SetID(4).SetFront(4, 0).SetRight(4, 0).SetBack(4, 0).SetLeft(4, 0).SetTop(4, 1).SetBottom(4, 1);
        public static Block Pebble_Block = new Block().SetID(5).SetFront(5, 0).SetRight(5, 0).SetLeft(5, 0).SetBack(5, 0).SetTop(5, 0).SetBottom(5, 0);

        public static Block Sand = new Block().SetID(7).SetFaces(5, 1, Block.ALL);

        public static Block Temp_Water = new Block().SetID(6).SetFront(4, 2).SetRight(4, 2).SetLeft(4, 2).SetBack(4, 2).SetTop(4, 2).SetBottom(4, 2);

        // TODO: replace with List<>
        public static Dictionary<int, Block> BlockIDs = new Dictionary<int, Block>();

        // TODO: make more auto (for loop)
        public static void RegisterIDs()
        {

            BlockIDs[Air.GetID()] = Air;
            BlockIDs[Grass.GetID()] = Grass;
            BlockIDs[Dirt.GetID()] = Dirt;
            BlockIDs[Stone.GetID()] = Stone;
            BlockIDs[Maple_Log.GetID()] = Maple_Log;
            BlockIDs[Pebble_Block.GetID()] = Pebble_Block;
            BlockIDs[Sand.GetID()] = Sand;

            BlockIDs[Temp_Water.GetID()] = Temp_Water;

            // BlockIDs[Dirt.GetID()] = Dirt;

        }

        public static Block GetBlockByID(int id)
        {

            // return BlockIDs[id];
            // this is temporary, support multiple later.
            return BlockLookups.DefaultLookup[id];

        }

        public static int GetIDByBlock(Block block)
        {

            return block.GetID();

        }

    }
}
