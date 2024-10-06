using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Util
{
    internal class JsonVector3ArrayConverter : JsonConverter<Vector3[]>
    {
        public override Vector3[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            int StartDepth = reader.CurrentDepth;

            List<Vector3> vertices = new List<Vector3>();

            while (reader.Read())
            {

                if (reader.TokenType == JsonTokenType.EndArray && reader.CurrentDepth == StartDepth)
                {

                    break;

                }

                if (reader.TokenType == JsonTokenType.StartArray)
                {

                    reader.Read();
                    float X = (float) reader.GetDecimal();
                    reader.Read();
                    float Y = (float)reader.GetDecimal();
                    reader.Read();
                    float Z = (float)reader.GetDecimal();

                    vertices.Add((X, Y, Z));

                    // Console.Log($"{X}, {Y}, {Z}");

                }

            }

            return vertices.ToArray();

        }

        public override void Write(Utf8JsonWriter writer, Vector3[] value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
