using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Util
{
    internal class JsonVertexConverter : JsonConverter<Vector3>
    {

        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

            return (positions[0], positions[1], positions[2]);

        }

        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {

            float[] vertex = new float[3];
            value.Deconstruct(out vertex[0], out vertex[1], out vertex[2]);

            writer.WriteStartArray("Point");
            writer.WriteNumberValue(vertex[0]);
            writer.WriteNumberValue(vertex[1]);
            writer.WriteNumberValue(vertex[2]);
            writer.WriteEndArray();

            // throw new NotImplementedException();
        }
    }
}
