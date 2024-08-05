using System.Collections.Generic;
using System;
using Blockgame_OpenTK.Util;
using System.Text.Json.Serialization;

namespace Blockgame_OpenTK.BlockUtil
{
    internal class Blocks
    {

        public static List<Block> BlockList = new List<Block>();

        public static readonly Block AirBlock = Block.LoadFromJson("AirBlock.json");
        public static readonly Block GrassBlock = Block.LoadFromJson("GrassBlock.json");
        public static readonly Block StoneBlock = Block.LoadFromJson("StoneBlock.json");
        public static void Load()
        {
            // Block b = Block.LoadFromJson("GrassBlockNew.json");
            // Globals.Register.AddBlock(AirBlock);
            Globals.Register.AddBlock(AirBlock);
            Globals.Register.AddBlock(GrassBlock);
            Globals.Register.AddBlock(StoneBlock);

        }

        public static int GetIDFromBlock(Block block)
        {

            return BlockList.IndexOf(block);
              
        }

        public static Block GetBlockFromID(int index)
        {

            Console.WriteLine(BlockList[index]);

            return BlockList[index];

        }

        public static Block GetBlockFromName(string name)
        {

            /* foreach (Block block in BlockList)
            {
                
                Console.WriteLine($"Block var name of {block.Name}: {block}");
                // if (block.Name == name) return block;

            }
            DebugMessage.WriteLine($"Block name {name} does not exist, returning Air instead.", DebugMessageType.Warning); */
            return null; 

        }

    }
}
