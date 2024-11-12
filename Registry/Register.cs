
using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Server;
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

        public Dictionary<ushort, Block> Blocks = new Dictionary<ushort, Block>();

        public Register() { }

        public void AddBlock(ushort id, Block block)
        {

            Debugger.Log($"Registering {block.DataName}", Severity.Info);
            block.ID = id;
            //BlockList.Add(block);
            Blocks.Add(block.ID, block);
            Debugger.Log($"Registered {block.DataName}", Severity.Info);

        }

        public Block GetBlockFromID(ushort id)
        {

            if (Blocks.ContainsKey(id))
            {

                return Blocks[id];

            }
            return Blocks[0];

        }

        public Block GetBlockFromName(string dataName)
        {

            foreach (Block block in Blocks.Values)
            {

                if (block.DataName == dataName) return block;

            }
            return null;

        }

        public ushort GetIDFromBlock(Block block)
        {

            return block.ID;

        }

        public ushort GetIDFromName(string name)
        {

            foreach (Block b in Blocks.Values)
            {

                if (b.DataName == name) return b.ID;

            }
            return Blocks[0].ID;

        }

        /*
        public void AddItem(Item item) 
        {

            ItemList.Add(item);

        }
         */

    }
}
