using Blockgame_OpenTK.Gui;
using Blockgame_OpenTK.Util;
using System;
using System.IO;
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
        public string SoundPath { get; set; }
        public ushort ID = 0;

        public AxisAlignedBoundingBox BoundingBox = new AxisAlignedBoundingBox((0, 0, 0), (1, 1, 1), (0, 0, 0));
        public GuiBlockModel GuiRenderableBlockModel;
        public static Block LoadFromJson(string fileName)
        {

            Block block = JsonSerializer.Deserialize<Block>(File.ReadAllText(Path.Combine(GlobalValues.BlockDataPath, fileName)));

            block.BoundingBox.StaticFriction = 0.8f;
            block.BoundingBox.DynamicFriction = 0.8f;
            // block.GuiRenderableBlockModel = new GuiBlockModel(block);

            return block;

        }
        public void OnBlockPlace() { }
        public void OnBlockInteract() { }
        public void OnBlockMine() { }
        public void OnBlockDestroy() { }
        public void OnTickUpdate() { }
        public void OnRandomTickUpdate() { }


    }
}
