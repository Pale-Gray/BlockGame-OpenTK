using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.Vulkan;
using OpenTK.Graphics.Vulkan.VulkanVideoCodecAv1std;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Transactions;

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

        public string InheritableName { get; set; }
        public string Inherit { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BlockModelCullDirection CullDirection { get; set; }
        [JsonConverter(typeof(JsonTextureIndexConverter))]
        public int? Texture { get; set; }
        public bool? ShouldRenderBackface { get; set; }
        [JsonConverter(typeof(JsonVector3Converter))]
        public Vector3? Normal { get; set; }
        [JsonConverter(typeof(JsonVector3ArrayConverter))]
        public Vector3[] Points { get; set; }
        [JsonConverter(typeof(JsonVector2ArrayConverter))]
        public Vector2[] TextureCoordinates { get; set; }
        [JsonConverter(typeof(JsonVector2Converter))]
        public Vector2? TextureOffset { get; set; }
        public float[] MovingWeights { get; set; }

    }

    public struct BlockModelAbstractData
    {

        public string Inherit { get; set; }
        [JsonConverter(typeof(JsonVector3Converter))]
        public Vector3? ModelRotation { get; set; }
        [JsonConverter(typeof(JsonVector3Converter))]
        public Vector3? ModelScale { get; set; }
        [JsonConverter(typeof(JsonVector3Converter))]
        public Vector3? ModelTranslation { get; set; }
        public BlockModelAbstractFaceData[] Faces { get; set; }

    }

    public struct BlockModelConvertedFaceData
    {

        public BlockModelCullDirection CullDirection;
        public ChunkVertex[] ReadableFace;

        public BlockModelConvertedFaceData(BlockModelCullDirection direction, ChunkVertex[] face)
        {

            CullDirection = direction;
            ReadableFace = face;

        }

    }

    internal class BlockModel
    {

        [JsonConverter(typeof(JsonBlockModelConverter))]
        public BlockModel Reference { get; set; }
        public BlockModelFace[] Faces { get; set; }
        public Dictionary<BlockModelCullDirection, ChunkVertex[]> ChunkReadableFaces = new Dictionary<BlockModelCullDirection, ChunkVertex[]>();
        public Dictionary<BlockModelCullDirection, ChunkVertex[]> UntransformedChunkReadableFaces = new Dictionary<BlockModelCullDirection, ChunkVertex[]>();
        public BlockModel()
        {



        }

        private static void PerformTransformations(BlockModelAbstractData modelData, Dictionary<BlockModelCullDirection, ChunkVertex[]> convertedFaces)
        {

            BlockModelAbstractData? inheritData = DeserializeInheritData(modelData.Inherit);
            Vector3 modelTranslation = inheritData?.ModelTranslation ?? modelData.ModelTranslation ?? Vector3.Zero;
            Vector3 modelRotation = inheritData?.ModelRotation ?? modelData.ModelRotation ?? Vector3.Zero;
            Vector3 modelScale = inheritData?.ModelScale ?? modelData.ModelScale ?? Vector3.One;

            if (modelScale != Vector3.One)
            {

                foreach (ChunkVertex[] convertedFace in convertedFaces.Values)
                {

                    for (int i = 0; i < convertedFace.Length; i++)
                    {

                        convertedFace[i].Position -= (0.5f, 0.5f, 0.5f);
                        convertedFace[i].Position *= modelScale;
                        convertedFace[i].Position += (0.5f, 0.5f, 0.5f);

                    }

                }

            }
            if (modelRotation != Vector3.Zero)
            {

                Matrix3 rotationMatrix = Matrix3.CreateFromQuaternion(Quaternion.FromEulerAngles(Maths.ToRadians(modelRotation)));
                foreach (ChunkVertex[] convertedFace in convertedFaces.Values)
                {

                    for (int i = 0; i < convertedFace.Length; i++)
                    {

                        convertedFace[i].Position -= (0.5f, 0.5f, 0.5f);
                        convertedFace[i].Position *= rotationMatrix;
                        convertedFace[i].Normal *= rotationMatrix;
                        convertedFace[i].Position += (0.5f, 0.5f, 0.5f);

                    }

                }

            }
            if (modelTranslation != Vector3.Zero)
            {

                foreach (ChunkVertex[] convertedFace in convertedFaces.Values)
                {

                    for (int i = 0; i < convertedFace.Length; i++)
                    {

                        convertedFace[i].Position -= (0.5f, 0.5f, 0.5f);
                        convertedFace[i].Position += modelTranslation / 32.0f;
                        convertedFace[i].Position += (0.5f, 0.5f, 0.5f);

                    }

                }

            }

        }

        private static Vector3 DetermineNormal(BlockModelAbstractFaceData face)
        {

            Vector3 tangent = face.Points[2] - face.Points[1];
            Vector3 bitangent = face.Points[0] - face.Points[1];
                
            return Vector3.Cross(tangent, bitangent).Normalized();

        }

        private static Vector2[] DetermineTextureCoordinates(BlockModelAbstractFaceData face)
        {
            
            if (face.TextureCoordinates == null)
            {

                Vector3 localRight = face.Points[2] - face.Points[1];
                Vector3 localUp = face.Points[0] - face.Points[1];

                Vector2[] coordinates =
                {

                    new Vector2(0, localUp.Length) / 32.0f,
                    new Vector2(0, 0) / 32.0f,
                    new Vector2(localRight.Length, 0) / 32.0f,
                    new Vector2(localRight.Length, localUp.Length) / 32.0f

                };

                return coordinates;

            } else
            {

                Vector2[] coordinates = face.TextureCoordinates;

                for (int i = 0; i < coordinates.Length; i++)
                {

                    coordinates[i] /= 32.0f;

                }

                return coordinates;

            }

        }
        private static ChunkVertex[] ConvertFace(string inheritModelName, BlockModelAbstractFaceData face)
        {

            ChunkVertex[] vertices = new ChunkVertex[6];

            // BlockModelAbstractData? inheritData = DeserializeData(inheritModelName);
            BlockModelAbstractData? inheritData = DeserializeInheritData(inheritModelName);

            if (face.Inherit != null)
            {

                foreach (BlockModelAbstractFaceData inheritFace in inheritData?.Faces)
                {

                    if (inheritFace.InheritableName == face.Inherit)
                    {

                        Vector2[] textureCoordinates = DetermineTextureCoordinates(inheritFace);
                        if (face.TextureOffset != null || inheritFace.TextureOffset != null)
                        {

                            for (int i = 0; i < textureCoordinates.Length; i++)
                            {

                                textureCoordinates[i] += (face.TextureOffset / 32.0f) ?? (inheritFace.TextureOffset / 32.0f) ?? Vector2.Zero;

                            }

                        }
                        Vector3 normal = DetermineNormal(inheritFace);

                        vertices[0] = new ChunkVertex(face.Texture ?? inheritFace.Texture ?? GlobalValues.ArrayTexture.GetTextureIndex("MissingTexture"), inheritFace.Points[0] / 32.0f, textureCoordinates[0], normal, 1);
                        vertices[1] = new ChunkVertex(face.Texture ?? inheritFace.Texture ?? GlobalValues.ArrayTexture.GetTextureIndex("MissingTexture"), inheritFace.Points[1] / 32.0f, textureCoordinates[1], normal, 1);
                        vertices[2] = new ChunkVertex(face.Texture ?? inheritFace.Texture ?? GlobalValues.ArrayTexture.GetTextureIndex("MissingTexture"), inheritFace.Points[2] / 32.0f, textureCoordinates[2], normal, 1);
                        vertices[3] = new ChunkVertex(face.Texture ?? inheritFace.Texture ?? GlobalValues.ArrayTexture.GetTextureIndex("MissingTexture"), inheritFace.Points[2] / 32.0f, textureCoordinates[2], normal, 1);
                        vertices[4] = new ChunkVertex(face.Texture ?? inheritFace.Texture ?? GlobalValues.ArrayTexture.GetTextureIndex("MissingTexture"), inheritFace.Points[3] / 32.0f, textureCoordinates[3], normal, 1);
                        vertices[5] = new ChunkVertex(face.Texture ?? inheritFace.Texture ?? GlobalValues.ArrayTexture.GetTextureIndex("MissingTexture"), inheritFace.Points[0] / 32.0f, textureCoordinates[0], normal, 1);

                    }

                }

            } else
            {

                Console.WriteLine("no inherit for this face!");

                Vector2[] textureCoordinates = DetermineTextureCoordinates(face);
                for (int i = 0; i < textureCoordinates.Length; i++)
                {

                    textureCoordinates[i] += (face.TextureOffset / 32.0f) ?? Vector2.Zero;

                }
                Vector3 normal = DetermineNormal(face);

                vertices[0] = new ChunkVertex(face.Texture ?? GlobalValues.ArrayTexture.GetTextureIndex("MissingTexture"), face.Points[0] / 32.0f, textureCoordinates[0], normal, 1);
                vertices[1] = new ChunkVertex(face.Texture ?? GlobalValues.ArrayTexture.GetTextureIndex("MissingTexture"), face.Points[1] / 32.0f, textureCoordinates[1], normal, 1);
                vertices[2] = new ChunkVertex(face.Texture ?? GlobalValues.ArrayTexture.GetTextureIndex("MissingTexture"), face.Points[2] / 32.0f, textureCoordinates[2], normal, 1);
                vertices[3] = new ChunkVertex(face.Texture ?? GlobalValues.ArrayTexture.GetTextureIndex("MissingTexture"), face.Points[2] / 32.0f, textureCoordinates[2], normal, 1);
                vertices[4] = new ChunkVertex(face.Texture ?? GlobalValues.ArrayTexture.GetTextureIndex("MissingTexture"), face.Points[3] / 32.0f, textureCoordinates[3], normal, 1);
                vertices[5] = new ChunkVertex(face.Texture ?? GlobalValues.ArrayTexture.GetTextureIndex("MissingTexture"), face.Points[0] / 32.0f, textureCoordinates[0], normal, 1);

            }

            return vertices;

        }
        private static Dictionary<BlockModelCullDirection, ChunkVertex[]> Convert(BlockModelAbstractData modelData, BlockModel model)
        {

            Dictionary<BlockModelCullDirection, ChunkVertex[]> faces = new Dictionary<BlockModelCullDirection, ChunkVertex[]>();

            foreach (BlockModelAbstractFaceData face in modelData.Faces)
            {

                BlockModelCullDirection cullDirection = BlockModelCullDirection.None;
                if (face.Inherit != null)
                {

                    BlockModelAbstractData? inheritModel = DeserializeInheritData(modelData.Inherit);
                    Console.WriteLine(inheritModel == null ? "no" : "yes");
                    foreach (BlockModelAbstractFaceData inheritFace in inheritModel?.Faces)
                    {

                        if (inheritFace.InheritableName == face.Inherit)
                        {

                            cullDirection = inheritFace.CullDirection;

                        }

                    }

                } else
                {

                    cullDirection = face.CullDirection;

                }
                // Console.WriteLine(face.InheritableName == null ? "null" : face.InheritableName);
                if (!faces.ContainsKey(cullDirection))
                {

                    faces.Add(cullDirection, ConvertFace(modelData.Inherit, face));

                }
                else
                {

                    List<ChunkVertex> current = faces[cullDirection].ToList();
                    current.AddRange(ConvertFace(modelData.Inherit, face));
                    faces[cullDirection] = current.ToArray();

                }

            }

            model.UntransformedChunkReadableFaces = faces;
            PerformTransformations(modelData, faces);

            return faces;

        }

        private static BlockModelAbstractData? DeserializeInheritData(string fileName)
        {

            //  BlockModelAbstractData? data = JsonSerializer.Deserialize<BlockModelAbstractData?>(File.ReadAllText(Path.Combine(GlobalValues.BlockModelPath, fileName)));

            Console.WriteLine(fileName);
            try
            {

                return JsonSerializer.Deserialize<BlockModelAbstractData>(File.ReadAllText(Path.Combine(GlobalValues.BlockModelPath, fileName)));

            } catch (Exception e) { Console.WriteLine(e.Message + e.StackTrace); return null; }

        }
        public static BlockModel LoadFromJson(string fileName)
        {

            BlockModelAbstractData modelData = JsonSerializer.Deserialize<BlockModelAbstractData>(File.ReadAllText(Path.Combine(GlobalValues.BlockModelPath, fileName)));

            float[] ambientValues = new float[4] { 1.0f, 1.0f, 1.0f, 1.0f };

            // BlockModel model = JsonSerializer.Deserialize<BlockModel>(File.ReadAllText(Path.Combine(GlobalValues.BlockModelPath, fileName)));

            BlockModel model = new BlockModel();
            Console.WriteLine(fileName);
            model.ChunkReadableFaces = Convert(modelData, model);
            // model.ChunkReadableFaces.Add(BlockModelCullDirection.Up, model.ConvertToChunkReadableFace(BlockModelCullDirection.Up));
            // model.ChunkReadableFaces.Add(BlockModelCullDirection.Down, model.ConvertToChunkReadableFace(BlockModelCullDirection.Down));
            // model.ChunkReadableFaces.Add(BlockModelCullDirection.Left, model.ConvertToChunkReadableFace(BlockModelCullDirection.Left));
            // model.ChunkReadableFaces.Add(BlockModelCullDirection.Right, model.ConvertToChunkReadableFace(BlockModelCullDirection.Right));
            // model.ChunkReadableFaces.Add(BlockModelCullDirection.Back, model.ConvertToChunkReadableFace(BlockModelCullDirection.Back));
            // model.ChunkReadableFaces.Add(BlockModelCullDirection.Front, model.ConvertToChunkReadableFace(BlockModelCullDirection.Front));
            // model.ChunkReadableFaces.Add(BlockModelCullDirection.None, model.ConvertToChunkReadableFace(BlockModelCullDirection.None, ambientValues));

            return model;

        }

        public float[] CalculateAmbientPoints(Dictionary<Vector3i, Chunk> worldChunks, Vector3i chunkPosition, BlockModelCullDirection direction, Vector3i blockPosition)
        {

            ChunkVertex[] face = UntransformedChunkReadableFaces[direction];
            float[] ambientPointData = { 0, 0, 0, 0 };

            Vector3 normal = face[0].Normal;
            if (normal == Vector3.UnitY || normal == -Vector3.UnitY || normal == Vector3.UnitX || normal == -Vector3.UnitX || normal == Vector3.UnitZ || normal == -Vector3.UnitZ)
            {

                Vector3i[] samplePoints = new Vector3i[8];
                bool[] sampleData = new bool[8];
                switch (normal)
                {

                    case (1, 0, 0):
                        samplePoints = [(1, 1, 1), (1, 0, 1), (1, -1, 1), (1, 1, 0), (1, -1, 0), (1, 1, -1), (1, 0, -1), (1, -1, -1)];
                        break;
                    case (-1, 0, 0):
                        samplePoints = [(-1, 1, -1), (-1, 0, -1), (-1, -1, -1), (-1, 1, 0), (-1, -1, 0), (-1, 1, 1), (-1, 0, 1), (-1, -1, 1)];
                        break;
                    case (0, 1, 0):
                        samplePoints = [(1, 1, 1), (1, 1, 0), (1, 1, -1), (0, 1, 1), (0, 1, -1), (-1, 1, 1), (-1, 1, 0), (-1, 1, -1)];
                        break;
                    case (0, -1, 0):
                        samplePoints = [(1, -1, -1), (1, -1, 0), (1, -1, 1), (0, -1, -1), (0, -1, 1), (-1, -1, -1), (-1, -1, 0), (-1, -1, 1)];
                        break;
                    case (0, 0, 1):
                        samplePoints = [(-1, 1, 1), (-1, 0, 1), (-1, -1, 1), (0, 1, 1), (0, -1, 1), (1, 1, 1), (1, 0, 1), (1, -1, 1)];
                        break;
                    case (0, 0, -1):
                        samplePoints = [(1, 1, -1), (1, 0, -1), (1, -1, -1), (0, 1, -1), (0, -1, -1), (-1, 1, -1), (-1, 0, -1), (-1, -1, -1)];
                        break;

                }

                for (int i = 0; i < 8; i++)
                {

                    Vector3i chunkPos = ChunkUtils.PositionToChunk((blockPosition + (32 * chunkPosition)) + samplePoints[i]);
                    Vector3i blockPos = ChunkUtils.PositionToBlockLocal((blockPosition + (32 * chunkPosition)) + samplePoints[i]);
                    if (worldChunks[chunkPos].GetBlockID(blockPos) != 0) sampleData[i] = true;

                }

                if (sampleData[0]) ambientPointData[0]++;
                if (sampleData[1]) { ambientPointData[0]++; ambientPointData[1]++; }
                if (sampleData[2]) ambientPointData[1]++;
                if (sampleData[3]) { ambientPointData[0]++; ambientPointData[3]++; }
                if (sampleData[4]) { ambientPointData[1]++; ambientPointData[2]++; }
                if (sampleData[5]) ambientPointData[3]++;
                if (sampleData[6]) { ambientPointData[2]++; ambientPointData[3]++; }
                if (sampleData[7]) ambientPointData[2]++;

            }

            return ambientPointData;

        }
        public ChunkVertex[] GetFaceWithOffsetAO(Dictionary<Vector3i, Chunk> worldChunks, Vector3i chunkPosition, BlockModelCullDirection direction, Vector3i offset)
        {

            ChunkVertex[] face = new ChunkVertex[ChunkReadableFaces[direction].Length];

            for (int i = 0; i < face.Length; i++)
            {

                face[i] = ChunkReadableFaces[direction][i];
                face[i].Position += offset;

            }

            float[] ambientPointData = CalculateAmbientPoints(worldChunks, chunkPosition, direction, offset);
            face[0].AmbientValue = ambientPointData[0];
            face[1].AmbientValue = ambientPointData[1];
            face[2].AmbientValue = ambientPointData[2];
            face[3].AmbientValue = ambientPointData[2];
            face[4].AmbientValue = ambientPointData[3];
            face[5].AmbientValue = ambientPointData[0];

            if (ambientPointData[0] != 0 || ambientPointData[2] != 0)
            {

                ChunkVertex[] vertices = new ChunkVertex[face.Length];
                for (int i = 0; i < face.Length; i++)
                {

                    vertices[i].Position = face[i].Position;
                    vertices[i].TextureCoordinates = face[i].TextureCoordinates;
                    vertices[i].AmbientValue = face[i].AmbientValue;

                }

                for (int i = 0; i < face.Length; i+=6)
                {

                    face[0].Position = vertices[i+1].Position;
                    face[1].Position = vertices[i+2].Position;
                    face[2].Position = vertices[i+4].Position;
                    face[3].Position = vertices[i+4].Position;
                    face[4].Position = vertices[i+5].Position;
                    face[5].Position = vertices[i+1].Position;

                    face[0].TextureCoordinates = vertices[i+1].TextureCoordinates;
                    face[1].TextureCoordinates = vertices[i + 2].TextureCoordinates;
                    face[2].TextureCoordinates = vertices[i + 4].TextureCoordinates;
                    face[3].TextureCoordinates = vertices[i + 4].TextureCoordinates;
                    face[4].TextureCoordinates = vertices[i + 5].TextureCoordinates;
                    face[5].TextureCoordinates = vertices[i + 1].TextureCoordinates;

                    face[0].AmbientValue = vertices[i+1].AmbientValue;
                    face[1].AmbientValue = vertices[i+2].AmbientValue;
                    face[2].AmbientValue = vertices[i+4].AmbientValue;
                    face[3].AmbientValue = vertices[i+4].AmbientValue;
                    face[4].AmbientValue = vertices[i+5].AmbientValue;
                    face[5].AmbientValue = vertices[i+1].AmbientValue;

                }

            }

            return face;

        }

        public ChunkVertex[] GetFaceWithOffset(BlockModelCullDirection direction, Vector3i offset)
        {

            ChunkVertex[] face = new ChunkVertex[ChunkReadableFaces[direction].Length];

            for (int i = 0; i < face.Length; i++)
            {

                face[i] = ChunkReadableFaces[direction][i];
                face[i].Position += offset;

            }

            return face;

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
