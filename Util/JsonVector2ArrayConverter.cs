using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blockgame_OpenTK.Util
{
    internal class JsonVector2ArrayConverter : JsonConverter<Vector2[]>
    {
        public override Vector2[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            int StartDepth = reader.CurrentDepth;

            List<Vector2> vertices = new List<Vector2>();

            while (reader.Read())
            {

                if (reader.TokenType == JsonTokenType.EndArray && reader.CurrentDepth == StartDepth)
                {

                    break;

                }

                if (reader.TokenType == JsonTokenType.StartArray)
                {

                    reader.Read();
                    float X = (float)reader.GetDecimal();
                    reader.Read();
                    float Y = (float)reader.GetDecimal();

                    vertices.Add((X, Y));

                    // Console.Log($"{X}, {Y}, {Z}");

                }

            }

            return vertices.ToArray();

        }

        public override void Write(Utf8JsonWriter writer, Vector2[] value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
