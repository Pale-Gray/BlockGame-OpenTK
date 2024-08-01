using Blockgame_OpenTK.ChunkUtil;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography.X509Certificates;
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
        [JsonPropertyName("Texture")]
        public int TextureIndex { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BlockModelNewCullDirection? CullDirection { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("Reference")]
        public BlockModelNewCullDirection? ReferenceCullDirection { get; set; }
        [JsonConverter(typeof(JsonVertexArrayConverter))]
        public Vector3[] Points { get; set; }

        public BlockModelNewFace()
        {

            TextureIndex = Globals.ArrayTexture.GetTextureIndex("MissingTexture");

        }

    }

    internal class BlockModel
    {

        [JsonConverter(typeof(JsonBlockModelNewConverter))]
        public BlockModel Reference { get; set; }
        public BlockModelNewFace[] Faces { get; set; }

        public BlockModel()
        {

            

        }
        public ChunkVertex[] ConvertToChunkReadableFaceOffset(Vector3 offset, BlockModelNewCullDirection referenceDirection)
        {

            ChunkVertex[] convertedVertices = ConvertToChunkReadableFace(referenceDirection);

            for (int i = 0; i < convertedVertices.Length; i++)
            {

                convertedVertices[i].Position += offset;

            }

            return convertedVertices;

        }
        public ChunkVertex[] ConvertToChunkReadableFace(BlockModelNewCullDirection referenceDirection)
        {

            List<ChunkVertex> vertices = new List<ChunkVertex>();

            foreach (var face in Faces)
            {


                if (face.ReferenceCullDirection != null && face.ReferenceCullDirection == referenceDirection)
                {

                    int textureIndex = face.TextureIndex;
                    foreach (var referenceFace in Reference.Faces)
                    {

                        if (referenceFace.CullDirection == referenceDirection)
                        {

                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[0], (0, 1), (0, 0, 0)));
                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[1], (0, 0), (0, 0, 0)));
                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[2], (1, 0), (0, 0, 0)));
                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[3], (1, 1), (0, 0, 0)));

                        }

                    }

                } else // This means that the reference direction is null, so find the regular cull direction
                {

                    if (face.CullDirection != null && face.CullDirection == referenceDirection)
                    {

                        int textureIndex = face.TextureIndex;
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[0], (0, 1), (0, 0, 0)));
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[1], (0, 0), (0, 0, 0)));
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[2], (1, 0), (0, 0, 0)));
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[3], (1, 1), (0, 0, 0)));

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
        public static ChunkVertex[] OffsetVertices(Vector3 offset, ChunkVertex[] vertices)
        {

            ChunkVertex[] verticesCopy = (ChunkVertex[]) vertices.Clone();
            for (int i = 0; i < verticesCopy.Length; i++)
            {

                verticesCopy[i].Position += offset;

            }
            return verticesCopy;

        }
        public static BlockModel LoadFromJson(string fileName)
        {

            // Console.WriteLine($"Deserializing {fileName}");
            return JsonSerializer.Deserialize<BlockModel>(File.ReadAllText(Globals.BlockModelPath + fileName));

        }

    }
}
