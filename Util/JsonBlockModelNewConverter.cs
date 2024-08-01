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
    internal class JsonBlockModelNewConverter : JsonConverter<BlockModel>
    {
        public override BlockModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            // Console.WriteLine($"Loading {reader.GetString()}");
            return BlockModel.LoadFromJson(reader.GetString());

        }

        public override void Write(Utf8JsonWriter writer, BlockModel value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
