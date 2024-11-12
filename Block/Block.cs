using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Gui;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blockgame_OpenTK.BlockUtil
{
    internal class Block
    {
        
        public string DisplayName { get; set; }
        public string DataName { get; set; }
        [JsonConverter(typeof(JsonBlockModelConverter))]
        [JsonPropertyName("Model")]
        public BlockModel BlockModel { get; set; }
        public bool? IsSolid { get; set; }
        public bool? HasCollision { get; set; }
        public ushort LightValue { get; set; }
        public string SoundPath { get; set; }
        public ushort ID = 0;

        public AxisAlignedBoundingBox BoundingBox = new AxisAlignedBoundingBox((0, 0, 0), (1, 1, 1), (0, 0, 0));
        public GuiBlockModel GuiRenderableBlockModel;
        public static Block LoadFromJson(string fileName)
        {

            Block block = JsonSerializer.Deserialize<Block>(File.ReadAllText(Path.Combine(GlobalValues.BlockDataPath, fileName)));

            block.BoundingBox.StaticFriction = 0.8f;
            block.BoundingBox.DynamicFriction = 0.8f;
            block.GuiRenderableBlockModel = new GuiBlockModel(block.BlockModel);

            return block;

        }

        public void SetProperties(Block block)
        {

            DisplayName = block.DisplayName;
            DataName = block.DataName;
            BlockModel = block.BlockModel;
            IsSolid = block.IsSolid;
            HasCollision = block.HasCollision;
            LightValue = block.LightValue;
            SoundPath = block.SoundPath;
            ID = block.ID;
            GuiRenderableBlockModel = block.GuiRenderableBlockModel;

            BoundingBox.StaticFriction = block.BoundingBox.StaticFriction;
            BoundingBox.DynamicFriction = block.BoundingBox.DynamicFriction;

        }

        public virtual void OnBlockSet(Chunk chunk, Vector3i localBlockPosition)
        {

            chunk.SetBlock(localBlockPosition, this);

        }
        public virtual void OnBlockPlace(World world, BlockProperties blockProperties, Vector3i globalBlockPosition)
        {

            world.SetBlock(globalBlockPosition, blockProperties, ID);

        }

        public virtual void OnBlockMesh(World world, BlockProperties properties, Vector3i globalBlockPosition)
        {

            ChunkMeshHandler.AddModel(world, globalBlockPosition, BlockModel);

        }
        public virtual void OnBlockInteract() { }
        public virtual void OnBlockMine() { }
        public virtual void OnBlockDestroy(World world, Vector3i globalBlockPosition)
        {

            world.SetBlock(globalBlockPosition, new BlockProperties(), 0);

        }
        public virtual void OnTickUpdate() { }
        public virtual void OnRandomTickUpdate() { }


    }
}
