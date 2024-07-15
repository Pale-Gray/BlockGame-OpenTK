
using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Registry
{
    internal class Register
    {

        public List<Block> BlockList = new List<Block>();
        // public List<Item> ItemList = new List<Item>();

        public Register() { }

        public void AddBlock(Block block)
        {

            DebugMessage.WriteLine($"Registering {block.DataName}", DebugMessageType.Info);
            BlockList.Add(block);
            DebugMessage.WriteLine($"Registered {block.DataName}", DebugMessageType.Info);

        }

        public Block GetBlockFromID(int id)
        {

            return BlockList[id];

        }

        public int GetIDFromBlock(Block block)
        {

            return BlockList.IndexOf(block);

        }

        public int GetIDFromName(string name)
        {

            return BlockList.IndexOf(BlockList.Find(b => b.DataName == name));

        }

        /*
        public void AddItem(Item item) 
        {

            ItemList.Add(item);

        }
         */

    }
}
