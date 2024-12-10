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
        public static readonly Block BrickBlock = Block.LoadFromJson("BrickBlock.json");
        public static readonly Block MissingBlock = Block.LoadFromJson("MissingBlock.json");
        public static readonly Block FoliageBlock = Block.LoadFromJson("FoliageBlock.json");
        //  public static readonly Block TallGrassBlock = new TallGrassBlock();
        // public static readonly Block LightBlock = new LightBlock();
        // public static readonly Block LogBranchBlock = new LogBranchBlock();
        public static readonly Block AspenLogBlock = Block.LoadFromJson("AspenLogBlock.json");
        public static readonly Block AspenLeafBlock = Block.LoadFromJson("AspenLeafBlock.json");
        // public static readonly Block LogFenceBlock = Block.LoadFromJson("LogFenceBlock.json");
        public static void Load()
        {
            
            GlobalValues.Register.AddBlock(0, AirBlock);
            GlobalValues.Register.AddBlock(1, GrassBlock);
            GlobalValues.Register.AddBlock(2, StoneBlock);
            GlobalValues.Register.AddBlock(3, DirtBlock);
            GlobalValues.Register.AddBlock(4, LogBlock);
            GlobalValues.Register.AddBlock(5, BrickBlock);
            GlobalValues.Register.AddBlock(6, MissingBlock);
            GlobalValues.Register.AddBlock(7, FoliageBlock);
            GlobalValues.Register.AddBlock(8, new AspenTreeBlock());
            GlobalValues.Register.AddBlock(9, AspenLeafBlock);
            GlobalValues.Register.AddBlock(10, AspenLogBlock);

            for (int i = 0; i < 5; i++)
            {

                Block block = GlobalValues.Register.GetBlockFromID((ushort)i);

                // Console.Log(block.DataName);
                // Console.Log(block.BlockModel.ChunkReadableFaces[BlockModelCullDirection.Up]);

            }

        }

    }
}
