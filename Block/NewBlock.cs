using Blockgame_OpenTK.ChunkUtil;
using Blockgame_OpenTK.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.BlockUtil
{
    internal class NewBlock
    {
        
        string DataName { get; set; }
        string DisplayName { get; set; }
        BlockModel Model { get; set; }
        [JsonPropertyName("Sounds")]
        string SoundPath { get; set; }
        int BreakTime { get; set; }


        public NewBlock()
        {



        }
        public static NewBlock LoadFromJson(string fileName)
        {

            return JsonSerializer.Deserialize<NewBlock>(Globals.BlockModelPath + fileName + ".json");

        }

    }
}
