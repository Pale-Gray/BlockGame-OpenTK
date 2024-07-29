using Blockgame_OpenTK.ChunkUtil;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blockgame_OpenTK.BlockUtil
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BlockFaceType
    {

        Up,
        Down,
        Left,
        Right,
        Front,
        Back,
        Extra

    }

    public struct BlockModelFace
    {
        public string Texture { get; set; }

        [JsonPropertyName("Points")]
        [JsonConverter(typeof(JsonVertexArrayConverter))]
        public required Vector3[] Points { get; set; }

    }
    internal class BlockModel
    {

        public bool NoCull { get; set; } = false;
        public Dictionary<BlockFaceType, BlockModelFace> Faces { get; set; }
        [JsonConstructor]
        public BlockModel()
        {



        }

        public ChunkVertex[] GetConvertedFace(BlockFaceType faceType)
        {

            // Stopwatch sw = Stopwatch.StartNew();
            List<ChunkVertex> vertices = new List<ChunkVertex>();

            BlockModelFace face = Faces[faceType];
            int textureIndex = Globals.ArrayTexture.GetTextureIndex(face.Texture);
            // Console.WriteLine(textureIndex);

            Vector3[] positions =
            {

                face.Points[0],
                face.Points[1],
                face.Points[2],
                face.Points[2],
                face.Points[3],
                face.Points[0]
            
            };
            Vector2[] textureCoordinates =
            {

                (0, 1),
                (0, 0),
                (1, 0),
                (1, 0),
                (1, 1),
                (0, 1)

            };

            for (int i = 0; i < positions.Length; i++)
            {

                vertices.Add(new ChunkVertex(textureIndex, positions[i], textureCoordinates[i], GetFaceNormal(faceType)));

            }

            // sw.Stop();
            // Console.WriteLine($"Finished in {sw.Elapsed}");
            return vertices.ToArray();

        }

        public Vector3 GetFaceNormal(BlockFaceType faceType)
        {

            switch(faceType)
            {

                case BlockFaceType.Up:
                    return Vector3.UnitY;
                case BlockFaceType.Down:
                    return -Vector3.UnitY;
                case BlockFaceType.Left:
                    return Vector3.UnitX;
                case BlockFaceType.Right:
                    return -Vector3.UnitX;
                case BlockFaceType.Back:
                    return Vector3.UnitZ;
                case BlockFaceType.Front:
                    return -Vector3.UnitZ;

            }

            return Vector3.Zero;

        }

        public ChunkVertex[] OffsetFace(ChunkVertex[] face, Vector3 offset)
        {

            ChunkVertex[] originalVertices = face;

            for (int i = 0; i < originalVertices.Length; i++)
            {

                originalVertices[i].Position += offset;

            }

            return originalVertices;

        }
        public static BlockModel Load(string fileName)
        {

            return JsonSerializer.Deserialize<BlockModel>(File.ReadAllText(Globals.BlockModelPath + fileName));

        }

    }

}
