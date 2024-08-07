﻿using Blockgame_OpenTK.ChunkUtil;
using Blockgame_OpenTK.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.BlockUtil
{
    internal class Block
    {
        
        public string DataName { get; set; }
        public string DisplayName { get; set; }
        [JsonConverter(typeof(JsonBlockModelNewConverter))]
        [JsonPropertyName("Model")]
        public BlockModel BlockModel { get; set; }
        [JsonPropertyName("Sounds")]
        public string SoundPath { get; set; }
        public int BreakTime { get; set; }
        public static Block LoadFromJson(string fileName)
        {

            return JsonSerializer.Deserialize<Block>(File.ReadAllText(Globals.BlockDataPath + fileName));

        }

        public void OnBlockPlace() { }
        public void OnBlockInteract() { }
        public void OnBlockMine() { }
        public void OnBlockDestroy() { }
        public void OnTickUpdate() { }
        public void OnRandomTickUpdate() { }


    }
}
