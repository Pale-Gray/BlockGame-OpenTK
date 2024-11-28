using Blockgame_OpenTK.BlockUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Util
{
    internal class JsonBlockPropertiesConverter : JsonConverter<BlockProperties>
    {
        public override BlockProperties Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            int startDepth = reader.CurrentDepth;

            BlockProperties defaultProperties = new BlockProperties();

            while (reader.Read())
            {

                if (reader.TokenType == JsonTokenType.EndObject || reader.CurrentDepth == startDepth) break;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {

                    string key = reader.GetString();
                    reader.Read();
                    object value = null;
                    switch (reader.TokenType)
                    {
                        case (JsonTokenType.String): value = reader.GetString(); break;
                        case (JsonTokenType.Number): value = reader.GetDouble(); break;
                        case (JsonTokenType.True): value = reader.GetBoolean(); break;
                        case (JsonTokenType.False): value = reader.GetBoolean(); break;
                    }
                    defaultProperties.AddProperty(key, value);

                }

            }

            return defaultProperties;

        }

        public override void Write(Utf8JsonWriter writer, BlockProperties value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

    }
}
