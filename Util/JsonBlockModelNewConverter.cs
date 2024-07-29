using Blockgame_OpenTK.BlockUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Util
{
    internal class JsonBlockModelNewConverter : JsonConverter<BlockModelNew>
    {
        public override BlockModelNew Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            // Console.WriteLine(reader.GetString());

            return JsonSerializer.Deserialize<BlockModelNew>(File.ReadAllText(Globals.BlockModelPath + reader.GetString()));

        }

        public override void Write(Utf8JsonWriter writer, BlockModelNew value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
