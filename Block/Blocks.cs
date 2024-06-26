﻿using System.Collections.Generic;
using System;
using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK.BlockUtil
{
    internal class Blocks
    {

        public static List<Block> BlockList = new List<Block>();

        public static Block Air = new Block("Air").SetFaceTexture(1, 0, 0).Register();   
         
        public static Block Grass = new Block("Grass").SetFaceTexture(1, 0, 0, 1, 2, 3).SetFaceTexture(0, 0, 5).SetFaceTexture(2, 0, 4).Register();
        public static Block Dirt = new Block("Dirt").SetFaceTexture(0, 0, 0, 1, 2, 3, 4, 5).Register();
        public static Block Stone = new Block("Stone").Register(); //.SetFaceTexture(3, 0, 0, 1, 2, 3, 4, 5).Register();
            
        public readonly static Block G = new Block("") { Name = "Grass" }.Register();

        public static int GetIDFromBlock(Block block)
        {

            return BlockList.IndexOf(block);
              
        }

        public static Block GetBlockFromID(int index)
        {

            return BlockList[index];

        }

        public static Block GetBlockFromName(string name)
        {

            foreach (Block block in BlockList)
            {

                Console.WriteLine($"Block var name of {block.Name}: {block}");
                // if (block.Name == name) return block;

            }
            DebugMessage.WriteLine($"Block name {name} does not exist, returning Air instead.", DebugMessageType.Warning);
            return Air;

        }

    }
}
