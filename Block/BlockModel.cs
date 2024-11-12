using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.Vulkan;
using OpenTK.Graphics.Vulkan.VulkanVideoCodecAv1std;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
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

    public struct BlockModelNewAbstractFaceData
    {

        public string Name { get; set; }
        public string Inherit { get; set; }
        public string Texture { get; set; }
        [JsonConverter(typeof(JsonVector3ArrayConverter))]
        public Vector3[] Points { get; set; }
        [JsonConverter(typeof(JsonVector2ArrayConverter))]
        public Vector2[] Uvs { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter<BlockModelCullDirection>))]
        public BlockModelCullDirection? CullDirection { get; set; }

    }

    public struct BlockModelNewAbstractData
    {

        public string Inherit { get; set; }
        [JsonConverter(typeof(JsonVector3Converter))]
        public Vector3? Translation { get; set; }
        [JsonConverter(typeof(JsonVector3Converter))]
        public Vector3? Rotation { get; set; }
        [JsonConverter(typeof(JsonVector3Converter))]
        public Vector3? Scale { get; set; }
        public Dictionary<string, string> Textures { get; set; }
        public BlockModelNewAbstractFaceData[] Faces { get; set; }

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

        public static Vector3[] ApplyTransformations(Vector3[] originalPoints, Vector3 translation, Vector3 rotation, Vector3 scale)
        {

            Vector3[] facePoints = originalPoints;
            for (int i = 0; i < facePoints.Length; i++)
            {

                facePoints[i] /= 32.0f;
                facePoints[i] -= (0.5f, 0.5f, 0.5f);
                if (rotation.X != 0) facePoints[i] *= Matrix3.CreateRotationX(Maths.ToRadians(rotation.X));
                if (rotation.Y != 0) facePoints[i] *= Matrix3.CreateRotationY(Maths.ToRadians(rotation.Y));
                if (rotation.Z != 0) facePoints[i] *= Matrix3.CreateRotationZ(Maths.ToRadians(rotation.Z));
                if (scale != Vector3.One) facePoints[i] *= scale;
                if (translation != Vector3.Zero) facePoints[i] += translation;
                facePoints[i] += (0.5f, 0.5f, 0.5f);

            }
            return facePoints;

        }

        public static Vector3 CalculateNormal(Vector3[] points)
        {

            Vector3[] transformedFace = points;
            Vector3 tangent = transformedFace[0] - transformedFace[1];
            Vector3 bitangent = transformedFace[1] - transformedFace[2];
            Console.WriteLine($"tangent {tangent}, bitangent {bitangent}");
            Vector3 normal = Vector3.Cross(tangent, bitangent);
            Console.WriteLine($"noraml {normal}");
            return normal;

        }

        public static Vector2[] CalculateTextureCoordinates(Vector3[] points)
        {

            Vector2[] autoUvs = new Vector2[4];

            float xLength = (points[0] - points[1]).Length;
            float yLength = (points[1] - points[2]).Length;

            autoUvs[0] = (0, yLength);
            autoUvs[1] = (0, 0);
            autoUvs[2] = (xLength, 0);
            autoUvs[3] = (xLength, yLength);

            return autoUvs;

        }

        public static BlockModelCullDirection DetermineCullDirection(Vector3 normal)
        {

            switch (normal)
            {

                case (0, 1, 0):
                    return BlockModelCullDirection.Up;
                case (0, -1, 0):
                    return BlockModelCullDirection.Down;
                case (1, 0, 0):
                    return BlockModelCullDirection.Left;
                case (-1, 0, 0):
                    return BlockModelCullDirection.Right;
                case (0, 0, 1):
                    return BlockModelCullDirection.Back;
                case (0, 0, -1):
                    return BlockModelCullDirection.Front;

            }

            return BlockModelCullDirection.None;

        }

        public static int DetermineTextureIndex(BlockModelNewAbstractData modelData, BlockModelNewAbstractData? inheritModelData, string textureReference)
        {

            if (modelData.Textures.ContainsKey(textureReference))
            {

                return GlobalValues.ArrayTexture.GetTextureIndex(modelData.Textures[textureReference]);

            }
            if (inheritModelData != null)
            {

                if ((bool) inheritModelData?.Textures.ContainsKey(textureReference))
                {

                    return GlobalValues.ArrayTexture.GetTextureIndex(inheritModelData?.Textures[textureReference]);

                }

            }
            return GlobalValues.ArrayTexture.GetTextureIndex("MissingTexture");

        }

        public static Dictionary<BlockModelCullDirection, ChunkVertex[]> Parse(BlockModelNewAbstractData abstractBlockModelData, BlockModel model)
        {

            Dictionary<BlockModelCullDirection, ChunkVertex[]> parsedFaces = new Dictionary<BlockModelCullDirection, ChunkVertex[]>();

            BlockModelNewAbstractData? inheritModel = null;
            if (abstractBlockModelData.Inherit != null) inheritModel = JsonSerializer.Deserialize<BlockModelNewAbstractData>(File.ReadAllText(Path.Combine(GlobalValues.BlockModelPath, abstractBlockModelData.Inherit)));

            foreach (BlockModelNewAbstractFaceData face in abstractBlockModelData.Faces)
            {

                ChunkVertex[] parsedVertices = new ChunkVertex[4];
                BlockModelNewAbstractFaceData? inheritedFaceData = null;
                if (face.Inherit != null)
                {

                    for (int i = 0; i < inheritModel?.Faces.Length; i++)
                    {

                        if (inheritModel?.Faces[i].Name == face.Inherit)
                        {

                            inheritedFaceData = inheritModel?.Faces[i];

                        }

                    }

                }

                Vector3[] points = face.Points ?? inheritedFaceData?.Points ?? new Vector3[4];
                Vector3[] transformedPoints = ApplyTransformations(points, abstractBlockModelData.Translation ?? inheritModel?.Translation ?? Vector3.Zero, abstractBlockModelData.Rotation ?? inheritModel?.Rotation ?? Vector3.Zero, abstractBlockModelData.Scale ?? inheritModel?.Scale ?? Vector3.One);
                Vector3 normal = CalculateNormal(transformedPoints);
                // Console.WriteLine(face.Inherit);
                Vector2[] textureCoordinates = face.Uvs ?? inheritedFaceData?.Uvs ?? CalculateTextureCoordinates(points);
                BlockModelCullDirection faceCullDirection = face.CullDirection ?? inheritedFaceData?.CullDirection ?? DetermineCullDirection(normal);
                int textureIndex = DetermineTextureIndex(abstractBlockModelData, inheritModel, face.Texture ?? inheritedFaceData?.Texture ?? null);

                for (int i = 0; i < parsedVertices.Length; i++)
                {

                    parsedVertices[i].Position = transformedPoints[i];
                    parsedVertices[i].Normal = normal;
                    parsedVertices[i].TextureCoordinates = textureCoordinates[i];
                    parsedVertices[i].TextureIndex = textureIndex;

                }

                for (int i = 0; i < parsedVertices.Length; i++)
                {

                    Console.WriteLine($"{parsedVertices[i].Position}, {parsedVertices[i].Normal}, {parsedVertices[i].TextureCoordinates}, {parsedVertices[i].TextureIndex}");

                }

                if (!parsedFaces.ContainsKey(faceCullDirection))
                {

                    parsedFaces.Add(faceCullDirection, parsedVertices);

                } else
                {

                    List<ChunkVertex> currentVertices = parsedFaces[faceCullDirection].ToList();
                    currentVertices.AddRange(parsedVertices);
                    parsedFaces[faceCullDirection] = currentVertices.ToArray();

                }

            }

            return parsedFaces;

        }

        public static BlockModel LoadFromJson(string jsonFileName)
        {

            Console.WriteLine(Path.Combine(GlobalValues.BlockModelPath, jsonFileName));
            BlockModelNewAbstractData data = JsonSerializer.Deserialize<BlockModelNewAbstractData>(File.ReadAllText(Path.Combine(GlobalValues.BlockModelPath, jsonFileName)));

            BlockModel model = new BlockModel();

            model.ChunkReadableFaces = Parse(data, model);
            // Console.WriteLine(model.ChunkReadableFaces.Count);

            return model;

        }

        public float[] DetermineAmbientValues(ConcurrentDictionary<Vector3i, Chunk> worldChunks, Vector3i chunkPosition, BlockModelCullDirection direction, Vector3i blockPosition)
        {

            float[] ambientValues = new float[4];

            Vector3i[] sampleDirections = new Vector3i[8];
            switch (direction)
            {

                case BlockModelCullDirection.Up:
                    sampleDirections = [ (1, 1, 1), (1, 1, 0), (1, 1, -1), (0, 1, 1), (0, 1, -1), (-1, 1, 1), (-1, 1, 0), (-1, 1, -1) ];
                    break;
                case BlockModelCullDirection.Down:
                    sampleDirections = [ (-1, -1, 1), (-1, -1, 0), (-1, -1, -1), (0, -1, 1), (0, -1, -1), (1, -1, 1), (1, -1, 0), (1, -1, -1) ];
                    break;
                case BlockModelCullDirection.Left:
                    sampleDirections = [ (1, 1, 1), (1, 0, 1), (1, -1, 1), (1, 1, 0), (1, -1, 0), (1, 1, -1), (1, 0, -1), (1, -1, -1) ];
                    break;
                case BlockModelCullDirection.Right:
                    sampleDirections = [ (-1, 1, -1), (-1, 0, -1), (-1, -1, -1), (-1, 1, 0), (-1, -1, 0), (-1, 1, 1), (-1, 0, 1), (-1, -1, 1) ];
                    break;
                case BlockModelCullDirection.Back:
                    sampleDirections = [ (-1, 1, 1), (-1, 0, 1), (-1, -1, 1), (0, 1, 1), (0, -1, 1), (1, 1, 1), (1, 0, 1), (1, -1, 1) ];
                    break;
                case BlockModelCullDirection.Front:
                    sampleDirections = [ (1, 1, -1), (1, 0, -1), (1, -1, -1), (0, 1, -1), (0, -1, -1), (-1, 1, -1), (-1, 0, -1), (-1, -1, -1) ];
                    break;

            }

            bool[] ambientBools = new bool[8];
            for (int i = 0; i < ambientBools.Length; i++)
            {

                if (worldChunks[ChunkUtils.PositionToChunk(blockPosition + sampleDirections[i])].SolidMask[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(blockPosition + sampleDirections[i]))]) ambientBools[i] = true;

            }

            if (ambientBools[0]) ambientValues[0]++;
            if (ambientBools[1]) { ambientValues[0]++; ambientValues[1]++; }
            if (ambientBools[2]) { ambientValues[1]++; }
            if (ambientBools[3]) { ambientValues[0]++; ambientValues[3]++; }
            if (ambientBools[4]) { ambientValues[1]++; ambientValues[2]++; }
            if (ambientBools[5]) ambientValues[3]++;
            if (ambientBools[6]) { ambientValues[2]++; ambientValues[3]++; }
            if (ambientBools[7]) ambientValues[2]++;

            return ambientValues;

        }
        public ChunkVertex[] GetFaceWithOffsetAO(ConcurrentDictionary<Vector3i, Chunk> worldChunks, Vector3i chunkPosition, BlockModelCullDirection direction, Vector3i offset)
        {

            ChunkVertex[] vertices = new ChunkVertex[ChunkReadableFaces[direction].Length];
            for (int i = 0; i < vertices.Length; i++)
            {

                vertices[i] = ChunkReadableFaces[direction][i];
                vertices[i].Position += offset;

            }

            float[] ambientValues = DetermineAmbientValues(worldChunks, chunkPosition, direction, offset + (chunkPosition * 32));

            vertices[0].AmbientValue = ambientValues[0];
            vertices[1].AmbientValue = ambientValues[1];
            vertices[2].AmbientValue = ambientValues[2];
            vertices[3].AmbientValue = ambientValues[3];
            if ((ambientValues[0] != 0 && ambientValues[1] == 0 && ambientValues[2] == 0 && ambientValues[3] == 0) || (ambientValues[0] == 0 && ambientValues[1] == 0 && ambientValues[2] != 0 && ambientValues[3] == 0))
            {

                ChunkVertex[] originalVertices = new ChunkVertex[vertices.Length];
                for (int i = 0; i < originalVertices.Length; i++)
                {

                    originalVertices[i] = vertices[i];

                }
                vertices[0] = originalVertices[3];
                vertices[1] = originalVertices[0];
                vertices[2] = originalVertices[1];
                vertices[3] = originalVertices[2];

            }

            for (int i = 0; i < vertices.Length / 4; i++)
            {

                int currentCount = (worldChunks[chunkPosition].IndicesList.Count / 6) * 4;
                int[] indices =
                {

                    0+currentCount,1+currentCount,2+currentCount,2+currentCount,3+currentCount,0+currentCount

                };
                worldChunks[chunkPosition].IndicesList.AddRange(indices);

            }
            return vertices;

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
