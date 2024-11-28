using Blockgame_OpenTK.BlockProperty;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Gui;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blockgame_OpenTK.BlockUtil
{

    internal struct BlockAbstractData
    {

        public string DisplayName { get; set; }
        public string DataName { get; set; }
        [JsonConverter(typeof(JsonBlockModelConverter))]
        [JsonPropertyName("Model")]
        public BlockModel BlockModel { get; set; }
        public bool? RenderAo { get; set; }
        public bool? IsSolid { get; set; }
        public bool? IsCullable { get; set; }
        public bool? HasCollision { get; set; }
        public ushort LightValue { get; set; }
        public string SoundPath { get; set; }
        public ushort ID;
        public Dictionary<string, object> BlockProperties { get; set; }

    }

    internal class Block
    {
        
        public string DisplayName { get; set; }
        public string DataName { get; set; }
        [JsonPropertyName("Model")]
        public BlockModel BlockModel { get; set; }
        public bool? RenderAo { get; set; }
        public bool? IsSolid { get; set; }
        public bool? IsCullable { get; set; }
        public bool? HasCollision { get; set; }
        public ushort LightValue { get; set; }
        public string SoundPath { get; set; }
        public ushort ID = 0;
        public BlockProperty.BlockProperties BlockProperties;
        // public BlockProperties BlockProperties { get; set; }

        public AxisAlignedBoundingBox BoundingBox = new AxisAlignedBoundingBox((0, 0, 0), (1, 1, 1), (0, 0, 0));
        public GuiBlockModel GuiRenderableBlockModel;
        public static Block LoadFromJson(string fileName)
        {

            Block block = JsonSerializer.Deserialize<Block>(File.ReadAllText(Path.Combine(GlobalValues.BlockDataPath, fileName)), GlobalValues.DefaultJsonOptions); 

            block.BoundingBox.StaticFriction = 0.8f;
            block.BoundingBox.DynamicFriction = 0.8f;
            block.GuiRenderableBlockModel = new GuiBlockModel(block.BlockModel);

            return block;

        }

        public virtual BlockProperty.BlockProperties CreateNewProperties()
        {

            BlockProperty.BlockProperties properties = new BlockProperty.BlockProperties();

            return properties;

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
            // BlockProperties = block.BlockProperties;

            BoundingBox.StaticFriction = block.BoundingBox.StaticFriction;
            BoundingBox.DynamicFriction = block.BoundingBox.DynamicFriction;

        }

        public virtual void OnBlockSet(Chunk chunk, Vector3i localBlockPosition)
        {

            // chunk.BlockPropertyNewData[ChunkUtils.VecToIndex(localBlockPosition)] = CreateNewProperties();
            chunk.SetBlock(localBlockPosition, CreateNewProperties(), this);

        }
        public virtual void OnBlockPlace(World world, Vector3i globalBlockPosition)
        {

            world.SetBlock(globalBlockPosition, CreateNewProperties(), ID);

        }

        public virtual void OnBlockMesh(World world, Dictionary<Vector3i, bool[]> mask, BlockProperty.BlockProperties properties, Vector3i globalBlockPosition)
        {

            world.AppendModel(BlockModel, globalBlockPosition, mask);

        }
        public virtual void OnBlockInteract() { }
        public virtual void OnBlockMine() { }
        public virtual void OnBlockDestroy(World world, Vector3i globalBlockPosition)
        {

            world.SetBlock(globalBlockPosition, new BlockProperty.BlockProperties(), 0);

        }
        public virtual void OnTickUpdate() { }
        public virtual void OnRandomTickUpdate(World world, Vector3i globalBlockPosition, BlockProperty.BlockProperties blockProperties)
        {
        
            
        
        }


    }
}
