using System.Collections.Generic;
using System;
using Blockgame_OpenTK.Util;
using System.Text.Json.Serialization; 

namespace Blockgame_OpenTK.BlockUtil
{
    internal class Blocks
    {

        public static readonly Block AirBlock = Block.LoadFromJson("AirBlock.json");
        public static readonly Block GrassBlock = Block.LoadFromJson("GrassBlock.json");
        public static readonly Block StoneBlock = Block.LoadFromJson("StoneBlock.json");
        public static readonly Block DirtBlock = Block.LoadFromJson("DirtBlock.json");
        public static readonly Block LogBlock = Block.LoadFromJson("LogBlock.json");
        public static void Load()
        {
            // Block b = Block.LoadFromJson("GrassBlockNew.json");
            // Globals.Register.AddBlock(AirBlock);
            GlobalValues.Register.AddBlock(0, AirBlock);
            GlobalValues.Register.AddBlock(1, GrassBlock);
            GlobalValues.Register.AddBlock(2, StoneBlock);
            GlobalValues.Register.AddBlock(3, DirtBlock);
            GlobalValues.Register.AddBlock(4, LogBlock);

        }

    }
}
