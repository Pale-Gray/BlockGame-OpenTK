using OpenTK.Mathematics;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using Blockgame_OpenTK.Util;
using Blockgame_OpenTK.Core.Chunks;
using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace Blockgame_OpenTK.BlockUtil
{
    public enum BlockModelCullDirection
    {

        Up,
        Down,
        Left,
        Right,
        Front,
        Back,
        None

    }
    public struct BlockModelFace
    {

        [JsonConverter(typeof(JsonTextureIndexConverter))]
        [JsonPropertyName("Texture")]
        public int TextureIndex { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BlockModelCullDirection? CullDirection { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("Reference")]
        public BlockModelCullDirection? ReferenceCullDirection { get; set; }
        [JsonConverter(typeof(JsonVector3ArrayConverter))]
        public Vector3[] Points { get; set; }

        public BlockModelFace()
        {

            TextureIndex = GlobalValues.ArrayTexture.GetTextureIndex("MissingTexture");

        }

    }

    public struct BlockModelAbstractFaceData
    {

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BlockModelCullDirection? InheritDirection { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BlockModelCullDirection? CullDirection { get; set; }
        [JsonConverter(typeof(JsonTextureIndexConverter))]
        public int Texture { get; set; }
        public bool? IsOpaque { get; set; }
        [JsonConverter(typeof(JsonVector3Converter))]
        public Vector3? Normal { get; set; }
        [JsonConverter(typeof(JsonVector3ArrayConverter))]
        public required Vector3[] Points { get; set; }
        [JsonConverter(typeof(JsonVector2ArrayConverter))]
        public Vector2[] UV { get; set; }
        public float[] MovingWeights { get; set; }

    }

    public struct BlockModelAbstractData
    {

        [JsonConverter(typeof(JsonBlockModelConverter))]
        internal BlockModel Inherit { get; set; }
        [JsonConverter(typeof(JsonVector3Converter))]
        public Vector3 ModelRotation { get; set; }
        [JsonConverter(typeof(JsonVector3Converter))]
        public Vector3 ModelScale { get; set; }
        [JsonConverter(typeof(JsonVector3Converter))]
        public Vector3 ModelTranslation { get; set; }
        public required BlockModelAbstractFaceData[] Faces { get; set; }

    }

    internal class BlockModel
    {

        [JsonConverter(typeof(JsonBlockModelConverter))]
        public BlockModel Reference { get; set; }
        public BlockModelFace[] Faces { get; set; }
        public Dictionary<BlockModelCullDirection, ChunkVertex[]> ChunkReadableFaces = new Dictionary<BlockModelCullDirection, ChunkVertex[]>();

        public BlockModel()
        {



        }

        private static ChunkVertex[] ConvertFace(BlockModel inheritModel, BlockModelAbstractFaceData face)
        {

            List<ChunkVertex> convertedFaceVertices = new List<ChunkVertex>();

            if (face.InheritDirection == null)
            {



            } else
            {



            }

            return convertedFaceVertices.ToArray();

        }
        private static Dictionary<BlockModelCullDirection, ChunkVertex[]> Convert(BlockModelAbstractData modelData)
        {

            Dictionary<BlockModelCullDirection, ChunkVertex[]> faces = new Dictionary<BlockModelCullDirection, ChunkVertex[]>();

            foreach (BlockModelAbstractFaceData face in modelData.Faces)
            {

                ChunkVertex[] convertedFace = ConvertFace(modelData.Inherit, face);
                BlockModelCullDirection direction = face.CullDirection ?? face.InheritDirection ?? BlockModelCullDirection.None;
                if (faces.ContainsKey(direction))
                {

                    List<ChunkVertex> currentFace = faces[direction].ToList();
                    currentFace.AddRange(convertedFace);
                    faces[direction] = currentFace.ToArray();

                } else
                {

                    faces.Add(direction, convertedFace);

                }

            }

            return faces;

        }

        public static BlockModel LoadFromJson(string fileName)
        {

            try
            {

                BlockModelAbstractData modelData = JsonSerializer.Deserialize<BlockModelAbstractData>(File.ReadAllText(Path.Combine(GlobalValues.BlockModelPath, fileName)));



            } catch { }

            float[] ambientValues = new float[4] { 1.0f, 1.0f, 1.0f, 1.0f };

            BlockModel model = JsonSerializer.Deserialize<BlockModel>(File.ReadAllText(Path.Combine(GlobalValues.BlockModelPath, fileName)));

            model.ChunkReadableFaces.Add(BlockModelCullDirection.Up, model.ConvertToChunkReadableFace(BlockModelCullDirection.Up));
            model.ChunkReadableFaces.Add(BlockModelCullDirection.Down, model.ConvertToChunkReadableFace(BlockModelCullDirection.Down));
            model.ChunkReadableFaces.Add(BlockModelCullDirection.Left, model.ConvertToChunkReadableFace(BlockModelCullDirection.Left));
            model.ChunkReadableFaces.Add(BlockModelCullDirection.Right, model.ConvertToChunkReadableFace(BlockModelCullDirection.Right));
            model.ChunkReadableFaces.Add(BlockModelCullDirection.Back, model.ConvertToChunkReadableFace(BlockModelCullDirection.Back));
            model.ChunkReadableFaces.Add(BlockModelCullDirection.Front, model.ConvertToChunkReadableFace(BlockModelCullDirection.Front));
            // model.ChunkReadableFaces.Add(BlockModelCullDirection.None, model.ConvertToChunkReadableFace(BlockModelCullDirection.None, ambientValues));

            return model;

        }

        public ChunkVertex[] ConvertToChunkReadableFaceOffset(Vector3 offset, BlockModelCullDirection referenceDirection, float[] ambientPoints)
        {

            ChunkVertex[] convertedVertices = ConvertToChunkReadableFace(referenceDirection, ambientPoints);

            for (int i = 0; i < convertedVertices.Length; i++)
            {

                convertedVertices[i].Position += offset;

            }

            return convertedVertices;

        }

        public ChunkVertex[] GetFaceOffsetted(BlockModelCullDirection direction, Vector3i offset, float[] ambientValues)
        {

            ChunkVertex[] referenceFace = ChunkReadableFaces[direction];
            ChunkVertex[] face = new ChunkVertex[6];

            if ((ambientValues[1] == 1 && ambientValues[3] == 1 && (ambientValues[0] == 0 || ambientValues[2] == 0)) || (ambientValues[1] == 0 && ambientValues[2] == 0 && ambientValues[3] == 0) || (ambientValues[0] == 0 && ambientValues[1] == 0 && ambientValues[3] == 0))
            {

                referenceFace = ConvertToChunkReadableFaceFlipped(direction);

            }
            for (int i = 0; i < face.Length; i++)
            {

                face[i] = referenceFace[i];
                face[i].Position += offset;

            }

            if ((ambientValues[1] == 1 && ambientValues[3] == 1 && (ambientValues[0] == 0 || ambientValues[2] == 0)) || (ambientValues[1] == 0 && ambientValues[2] == 0 && ambientValues[3] == 0) || (ambientValues[0] == 0 && ambientValues[1] == 0 && ambientValues[3] == 0))
            {

                face[0].AmbientValue = ambientValues[1];
                face[1].AmbientValue = ambientValues[2];
                face[2].AmbientValue = ambientValues[3];
                face[3].AmbientValue = ambientValues[3];
                face[4].AmbientValue = ambientValues[0];
                face[5].AmbientValue = ambientValues[1];

            } else
            {

                face[0].AmbientValue = ambientValues[0];
                face[1].AmbientValue = ambientValues[1];
                face[2].AmbientValue = ambientValues[2];
                face[3].AmbientValue = ambientValues[2];
                face[4].AmbientValue = ambientValues[3];
                face[5].AmbientValue = ambientValues[0];

            }

            return face;

        }
        public ChunkVertex[] ConvertToChunkReadableFace(BlockModelCullDirection referenceDirection, float[] ambientPoints)
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

                            Vector3 normal = DetermineNormal(referenceDirection);
                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[0], (0, 1), normal, ambientPoints[0]));
                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[1], (0, 0), normal, ambientPoints[1]));
                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[2], (1, 0), normal, ambientPoints[2]));
                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[3], (1, 1), normal, ambientPoints[3]));

                        }

                    }

                } else // This means that the reference direction is null, so find the regular cull direction
                {

                    if (face.CullDirection != null && face.CullDirection == referenceDirection)
                    {

                        int textureIndex = face.TextureIndex;
                        Vector3 normal = DetermineNormal(referenceDirection);
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[0], (0, 1), normal, ambientPoints[0]));
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[1], (0, 0), normal, ambientPoints[1]));
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[2], (1, 0), normal, ambientPoints[2]));
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[3], (1, 1), normal, ambientPoints[3]));

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

        public ChunkVertex[] ConvertToChunkReadableFace(BlockModelCullDirection referenceDirection)
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

                            Vector3 normal = DetermineNormal(referenceDirection);
                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[0], (0, 1), normal, 1));
                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[1], (0, 0), normal, 1));
                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[2], (1, 0), normal, 1));
                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[3], (1, 1), normal, 1));

                        }

                    }

                }
                else // This means that the reference direction is null, so find the regular cull direction
                {

                    if (face.CullDirection != null && face.CullDirection == referenceDirection)
                    {

                        int textureIndex = face.TextureIndex;
                        Vector3 normal = DetermineNormal(referenceDirection);
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[0], (0, 1), normal, 1));
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[1], (0, 0), normal, 1));
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[2], (1, 0), normal, 1));
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[3], (1, 1), normal, 1));

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

        public ChunkVertex[] ConvertToChunkReadableFaceFlipped(BlockModelCullDirection referenceDirection)
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

                            Vector3 normal = DetermineNormal(referenceDirection);
                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[1], (0, 0), normal, 1));
                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[2], (1, 0), normal, 1));
                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[3], (1, 1), normal, 1));
                            vertices.Add(new ChunkVertex(textureIndex, referenceFace.Points[0], (0, 1), normal, 1));

                        }

                    }

                }
                else // This means that the reference direction is null, so find the regular cull direction
                {

                    if (face.CullDirection != null && face.CullDirection == referenceDirection)
                    {

                        int textureIndex = face.TextureIndex;
                        Vector3 normal = DetermineNormal(referenceDirection);
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[1], (0, 0), normal, 1));
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[2], (1, 0), normal, 1));
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[3], (1, 1), normal, 1));
                        vertices.Add(new ChunkVertex(textureIndex, face.Points[0], (0, 1), normal, 1));

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

        private static Vector3 DetermineNormal(BlockModelCullDirection cullDirection)
        {

            switch (cullDirection)
            {

                case BlockModelCullDirection.Up:
                    return Vector3.UnitY;
                case BlockModelCullDirection.Down:
                    return -Vector3.UnitY;
                case BlockModelCullDirection.Left:
                    return Vector3.UnitX;
                case BlockModelCullDirection.Right:
                    return -Vector3.UnitX;
                case BlockModelCullDirection.Front:
                    return -Vector3.UnitZ;
                case BlockModelCullDirection.Back:
                    return Vector3.UnitZ;
                default:
                    return Vector3.Zero;

            }

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

    }
}
