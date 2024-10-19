using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Util
{
    internal class JsonVector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            List<float> positions = new List<float>();

            int StartDepth = reader.CurrentDepth;

            while (reader.Read())
            {

                if (reader.TokenType == JsonTokenType.EndArray && reader.CurrentDepth == StartDepth)
                {

                    break;

                }

                if (reader.TokenType == JsonTokenType.Number)
                {

                    positions.Add((float)reader.GetDouble());

                }

                // Console.Log(reader.TokenType);

            }

            /* foreach (float point in positions)
            {

                Console.Log(point);

            } */

            return (positions[0], positions[1]);

        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
