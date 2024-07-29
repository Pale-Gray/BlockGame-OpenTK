using Blockgame_OpenTK.ChunkUtil;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blockgame_OpenTK.BlockUtil
{
    public enum BlockModelNewCullDirection
    {

        Up,
        Down,
        Left,
        Right,
        Front,
        Back,
        None

    }
    public struct BlockModelNewFace
    {

        [JsonConverter(typeof(JsonTextureIndexConverter))]
        public int TextureIndex { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BlockModelNewCullDirection CullDirection { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("Reference")]
        public BlockModelNewCullDirection? ReferenceCullDirection { get; set; }
        [JsonConverter(typeof(JsonVertexArrayConverter))]
        public Vector3[] Points { get; set; }

    }

    internal class BlockModelNew
    {

        [JsonConverter(typeof(JsonBlockModelNewConverter))]
        public BlockModelNew Reference { get; set; }
        public BlockModelNewFace[] Faces { get; set; }

        public BlockModelNew()
        {

            // Console.WriteLine($"Reference is {(Reference == null ? "Null":"Not Null")}");

        }

        public ChunkVertex[] ConvertToChunkReadableFace(BlockModelNewCullDirection referenceDirection)
        {

            List<ChunkVertex> vertices = new List<ChunkVertex>();

            foreach (var face in Faces)
            {

                if (face.ReferenceCullDirection != null)
                {

                    if (face.ReferenceCullDirection == referenceDirection)
                    {

                        int textureIndex = face.TextureIndex;
                        foreach (var referenceFace in Reference.Faces)
                        {

                            if (referenceFace.CullDirection == referenceDirection)
                            {

                                foreach (var point in referenceFace.Points)
                                {

                                    vertices.Add(new ChunkVertex(textureIndex, point, (0,0), (0,0,0)));

                                }

                            }

                        }

                    }

                } else
                {

                    if (face.CullDirection == referenceDirection)
                    {

                        foreach (var point in face.Points)
                        {

                            vertices.Add(new ChunkVertex(face.TextureIndex, point, (0,0), (0,0,0)));

                        }

                    } else
                    {

                        continue;

                    }

                }

            }

            ChunkVertex[] verticesToReturn =
            {

                vertices[0],
                vertices[1],
                vertices[2],
                vertices[2],
                vertices[3],
                vertices[0]

            };

            return verticesToReturn;

        }

        public static BlockModelNew LoadFromJson(string fileName)
        {

            return JsonSerializer.Deserialize<BlockModelNew>(File.ReadAllText(Globals.BlockModelPath + fileName));

        }

    }
}
