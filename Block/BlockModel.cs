using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
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

    public struct BlockModelCubePrimitiveFacePropertyData
    {

        public bool? IsVisible { get; set; }
        public bool? ShouldRenderAO { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter<BlockModelCullDirection>))]
        public BlockModelCullDirection? CullDirection { get; set; }
        public string Texture { get; set; }

    }

    public struct BlockModelCubePrimitiveData
    {

        public Vector3? Translation { get; set; }
        public Vector3? Rotation { get; set; }
        public Vector3? Scale { get; set; }
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }
        public Dictionary<string, BlockModelCubePrimitiveFacePropertyData> Properties { get; set; }

    }
    public struct BlockModelAbstractData
    {

        public string Inherit { get; set; }
        public Dictionary<string, string> Textures { get; set; }
        public BlockModelCubePrimitiveData[] Objects { get; set; }

    }

    public struct BlockModelData
    {

        public ChunkVertex[] VertexData;
        public bool ShouldRenderAO;

        public BlockModelData(ChunkVertex[] vertexData, bool shouldRenderAO)
        {

            VertexData = vertexData;
            ShouldRenderAO = shouldRenderAO;

        }

    }

    internal class BlockModel
    {
        public BlockModel Reference { get; set; }
        public BlockModelFace[] Faces { get; set; }
        public Dictionary<BlockModelCullDirection, ChunkVertex[]> ChunkReadableFaces = new Dictionary<BlockModelCullDirection, ChunkVertex[]>();
        public Dictionary<BlockModelCullDirection, BlockModelData> ReadableFaces = new Dictionary<BlockModelCullDirection, BlockModelData>();
        public Dictionary<BlockModelCullDirection, ChunkVertex[]> UntransformedChunkReadableFaces = new Dictionary<BlockModelCullDirection, ChunkVertex[]>();
        public BlockModel()
        {



        }

        public static ChunkVertex[] ConvertFace(Vector3[] positions, Vector2[] textureCoordinates, Vector3 normal, int textureIndex)
        {

            ChunkVertex[] face = new ChunkVertex[4];

            for (int i = 0; i < face.Length; i++)
            {

                face[i] = new ChunkVertex(textureIndex, positions[i], textureCoordinates[i], normal, 0.0f);

            }

            return face;

        }

        public static Vector3 CalculateNormalFromFace(Vector3[] face)
        {

            Vector3 normal = Vector3.Zero;

            Vector3 bitangent = face[0] - face[1];
            Vector3 tangent = face[1] - face[2];

            normal = Vector3.Cross(bitangent, tangent).Normalized();

            Console.WriteLine($"bitangent: {bitangent}, tangent: {tangent}, normal: {normal}");

            return normal;

        }

        public static Vector3[] TransformFace(Vector3[] face, Vector3 scale, Vector3 rotation, Vector3 translation)
        {

            Vector3[] transformedFaces = new Vector3[face.Length];

            for (int i = 0; i < transformedFaces.Length; i++)
            {

                transformedFaces[i] = face[i];

                transformedFaces[i] -= (0.5f, 0.5f, 0.5f);

                if (scale.X != 0) transformedFaces[i].X *= scale.X;
                if (scale.Y != 0) transformedFaces[i].Y *= scale.Y;
                if (scale.Z != 0) transformedFaces[i].Z *= scale.Z;
                if (rotation.X != 0) transformedFaces[i] *= Matrix3.CreateRotationX(Maths.ToRadians(rotation.X));
                if (rotation.Y != 0) transformedFaces[i] *= Matrix3.CreateRotationY(Maths.ToRadians(rotation.Y));
                if (rotation.Z != 0) transformedFaces[i] *= Matrix3.CreateRotationZ(Maths.ToRadians(rotation.Z));
                if (translation != Vector3.Zero) transformedFaces[i] += translation;

                transformedFaces[i] += (0.5f, 0.5f, 0.5f);

            }

            return transformedFaces;

        }
        public static Dictionary<string, Vector2[]> CalculateTextureCoordinates(BlockModelCubePrimitiveData cube)
        {

            Dictionary<string, Vector2[]> textureCoordinates = new Dictionary<string, Vector2[]>();

            Vector2[] topFace = [
                (1 - cube.End.X, cube.End.Z),
                (1 - cube.End.X, cube.Start.Z),
                (1 - cube.Start.X, cube.Start.Z),
                (1 - cube.Start.X, cube.End.Z)
            ];
            Vector2[] bottomFace = [
                (1 - cube.End.X, 1 - cube.Start.Z),
                (1 - cube.End.X, cube.End.Z),
                (1 - cube.Start.X, cube.End.Z),
                (1 - cube.Start.X, 1 - cube.Start.Z)
            ];
            Vector2[] frontFace = [
                (1 - cube.End.X, cube.End.Y),
                (1 - cube.End.X, cube.Start.Y),
                (1 - cube.Start.X, cube.Start.Y),
                (1 - cube.Start.X, cube.End.Y)
            ];
            Vector2[] leftFace = [
                (1 - cube.End.Z, cube.End.Y),
                (1 - cube.End.Z, cube.Start.Y),
                (1 - cube.Start.Z, cube.Start.Y),
                (1 - cube.Start.Z, cube.End.Y)
            ];
            Vector2[] backFace = [
                (cube.Start.X, cube.End.Y),
                (cube.Start.X, cube.Start.Y),
                (cube.End.X, cube.Start.Y),
                (cube.End.X, cube.End.Y)
            ];
            Vector2[] rightFace = [
                (cube.Start.Z, cube.End.Y),
                (cube.Start.Z, cube.Start.Y),
                (cube.End.Z, cube.Start.Y),
                (cube.End.Z, cube.End.Y)
            ];

            textureCoordinates.Add("Top", topFace);
            textureCoordinates.Add("Bottom", bottomFace);
            textureCoordinates.Add("Front", frontFace);
            textureCoordinates.Add("Left", leftFace);
            textureCoordinates.Add("Back", backFace);
            textureCoordinates.Add("Right", rightFace);

            return textureCoordinates;

        }

        public static int GetTextureIndexFromCube(Dictionary<string, string> textureList, BlockModelCubePrimitiveData cube, string face)
        {

            if (textureList.ContainsKey(cube.Properties[face].Texture)) return GlobalValues.ArrayTexture.GetTextureIndex(textureList[cube.Properties[face].Texture]);

            return GlobalValues.ArrayTexture.GetTextureIndex("MissingTexture");

        }
        public static void ParseCube(BlockModelCubePrimitiveData cube, BlockModel model, Dictionary<string, string> textures)
        {

            Vector3 start = cube.Start / 32.0f;
            Vector3 end = cube.End / 32.0f;

            // in order
            // top, bottom, front, left, back, right
            bool[] shouldFaceBeAdded = 
            {
                cube.Properties["Top"].IsVisible ?? true,
                cube.Properties["Bottom"].IsVisible ?? true,
                cube.Properties["Front"].IsVisible ?? true,
                cube.Properties["Left"].IsVisible ?? true,
                cube.Properties["Back"].IsVisible ?? true,
                cube.Properties["Right"].IsVisible ?? true
            };

            if (start.X == end.X)
            {

                shouldFaceBeAdded[0] = false;
                shouldFaceBeAdded[1] = false;
                shouldFaceBeAdded[2] = false;
                shouldFaceBeAdded[4] = false;

            }

            if (start.Y == end.Y)
            {

                shouldFaceBeAdded[2] = false;
                shouldFaceBeAdded[3] = false;
                shouldFaceBeAdded[4] = false;
                shouldFaceBeAdded[5] = false;

            }

            if (start.Z == end.Z)
            {

                shouldFaceBeAdded[0] = false;
                shouldFaceBeAdded[1] = false;
                shouldFaceBeAdded[3] = false;
                shouldFaceBeAdded[5] = false;

            }

            // in order
            // top, bottom, front, left, back, right
            BlockModelCullDirection?[] noCullDirection =
            {

                null,
                null,
                null,
                null,
                null,
                null

            };

            if (end.Y < 1.0f) noCullDirection[0] = BlockModelCullDirection.None;
            if (start.Y > 0.0f) noCullDirection[1] = BlockModelCullDirection.None;
            if (start.Z > 0.0f) noCullDirection[2] = BlockModelCullDirection.None;
            if (end.X < 1.0f) noCullDirection[3] = BlockModelCullDirection.None;
            if (end.Z < 1.0f) noCullDirection[4] = BlockModelCullDirection.None;
            if (start.X > 1.0f) noCullDirection[5] = BlockModelCullDirection.None;

            Vector3[] topFace = 
            {
                end,
                (end.X, end.Y, start.Z),
                (start.X, end.Y, start.Z),
                (start.X, end.Y, end.Z)
            };

            Vector2[] topFaceTextureCoordinates = 
            {
                (1 - end.X, end.Z),
                (1 - end.X, start.Z),
                (1 - start.X, start.Z),
                (1 - start.X, end.Z)
            };

            topFace = TransformFace(topFace, cube.Scale ?? Vector3.One, cube.Rotation ?? Vector3.Zero, cube.Translation ?? Vector3.Zero);

            int topTextureIndex = GetTextureIndexFromCube(textures, cube, "Top");
            Vector3 topNormal = CalculateNormalFromFace(topFace);
            BlockModelCullDirection topDirection = cube.Properties["Top"].CullDirection ?? noCullDirection[0] ?? DetermineCullDirection(topNormal);

            ChunkVertex[] topVertices =
            {

                new ChunkVertex(topTextureIndex, topFace[0], topFaceTextureCoordinates[0], topNormal, 0.0f, cube.Properties["Top"].ShouldRenderAO ?? true),
                new ChunkVertex(topTextureIndex, topFace[1], topFaceTextureCoordinates[1], topNormal, 0.0f, cube.Properties["Top"].ShouldRenderAO ?? true),
                new ChunkVertex(topTextureIndex, topFace[2], topFaceTextureCoordinates[2], topNormal, 0.0f, cube.Properties["Top"].ShouldRenderAO ?? true),
                new ChunkVertex(topTextureIndex, topFace[3], topFaceTextureCoordinates[3], topNormal, 0.0f, cube.Properties["Top"].ShouldRenderAO ?? true)

            };

            if (shouldFaceBeAdded[0])
            {

                if (!model.ChunkReadableFaces.ContainsKey(topDirection))
                {

                    model.ChunkReadableFaces.Add(topDirection, topVertices);

                }
                else
                {

                    List<ChunkVertex> currentVertices = model.ChunkReadableFaces[topDirection].ToList();
                    currentVertices.AddRange(topVertices);
                    model.ChunkReadableFaces[topDirection] = currentVertices.ToArray();

                }

            }

            Vector3[] bottomFace =
            {
                (end.X, start.Y, start.Z),
                (end.X, start.Y, end.Z),
                (start.X, start.Y, end.Z),
                (start.X, start.Y, start.Z)
            };

            Vector2[] bottomFaceTextureCoordinates =
            {
                (1 - end.X, 1 - start.Z),
                (1 - end.X, 1 - end.Z),
                (1 - start.X, 1 - end.Z),
                (1 - start.X, 1 - start.Z)
            };

            bottomFace = TransformFace(bottomFace, cube.Scale ?? Vector3.One, cube.Rotation ?? Vector3.Zero, cube.Translation ?? Vector3.Zero);

            int bottomTextureIndex = GetTextureIndexFromCube(textures, cube, "Bottom");
            Vector3 bottomNormal = CalculateNormalFromFace(bottomFace);
            BlockModelCullDirection bottomCullDirection = cube.Properties["Bottom"].CullDirection ?? noCullDirection[1] ?? DetermineCullDirection(bottomNormal);

            ChunkVertex[] bottomVertices =
            {

                new ChunkVertex(bottomTextureIndex, bottomFace[0], bottomFaceTextureCoordinates[0], bottomNormal, 0.0f, cube.Properties["Bottom"].ShouldRenderAO ?? true),
                new ChunkVertex(bottomTextureIndex, bottomFace[1], bottomFaceTextureCoordinates[1], bottomNormal, 0.0f, cube.Properties["Bottom"].ShouldRenderAO ?? true),
                new ChunkVertex(bottomTextureIndex, bottomFace[2], bottomFaceTextureCoordinates[2], bottomNormal, 0.0f, cube.Properties["Bottom"].ShouldRenderAO ?? true),
                new ChunkVertex(bottomTextureIndex, bottomFace[3], bottomFaceTextureCoordinates[3], bottomNormal, 0.0f, cube.Properties["Bottom"].ShouldRenderAO ?? true)

            };

            if (shouldFaceBeAdded[1])
            {

                if (!model.ChunkReadableFaces.ContainsKey(bottomCullDirection))
                {

                    model.ChunkReadableFaces.Add(bottomCullDirection, bottomVertices);

                }
                else
                {

                    List<ChunkVertex> currentVertices = model.ChunkReadableFaces[bottomCullDirection].ToList();
                    currentVertices.AddRange(bottomVertices);
                    model.ChunkReadableFaces[bottomCullDirection] = currentVertices.ToArray();

                }

            }

            Vector3[] frontFace =
            {
                (end.X, end.Y, start.Z),
                (end.X, start.Y, start.Z),
                start,
                (start.X, end.Y, start.Z)
            };

            Vector2[] frontFaceTextureCoordinates =
            {
                (1 - end.X, end.Y),
                (1 - end.X, start.Y),
                (1 - start.X, start.Y),
                (1 - start.X, end.Y)
            };

            frontFace = TransformFace(frontFace, cube.Scale ?? Vector3.One, cube.Rotation ?? Vector3.Zero, cube.Translation ?? Vector3.Zero);

            int frontTextureIndex = GetTextureIndexFromCube(textures, cube, "Front");
            Vector3 frontNormal = CalculateNormalFromFace(frontFace);
            BlockModelCullDirection frontCullDirection = cube.Properties["Front"].CullDirection ?? noCullDirection[2] ?? DetermineCullDirection(frontNormal);

            ChunkVertex[] frontVertices =
            {

                new ChunkVertex(frontTextureIndex, frontFace[0], frontFaceTextureCoordinates[0], frontNormal, 0.0f, cube.Properties["Front"].ShouldRenderAO ?? true),
                new ChunkVertex(frontTextureIndex, frontFace[1], frontFaceTextureCoordinates[1], frontNormal, 0.0f, cube.Properties["Front"].ShouldRenderAO ?? true),
                new ChunkVertex(frontTextureIndex, frontFace[2], frontFaceTextureCoordinates[2], frontNormal, 0.0f, cube.Properties["Front"].ShouldRenderAO ?? true),
                new ChunkVertex(frontTextureIndex, frontFace[3], frontFaceTextureCoordinates[3], frontNormal, 0.0f, cube.Properties["Front"].ShouldRenderAO ?? true)

            };

            if (shouldFaceBeAdded[2])
            {

                if (!model.ChunkReadableFaces.ContainsKey(frontCullDirection))
                {

                    model.ChunkReadableFaces.Add(frontCullDirection, frontVertices);

                }
                else
                {

                    List<ChunkVertex> currentVertices = model.ChunkReadableFaces[frontCullDirection].ToList();
                    currentVertices.AddRange(frontVertices);
                    model.ChunkReadableFaces[frontCullDirection] = currentVertices.ToArray();

                }

            }

            Vector3[] leftFace =
            {
                end,
                (end.X, start.Y, end.Z),
                (end.X, start.Y, start.Z),
                (end.X, end.Y, start.Z)
            };

            Vector2[] leftFaceTextureCoordinates =
            {
                (1 - end.Z, end.Y),
                (1 - end.Z, start.Y),
                (1 - start.Z, start.Y),
                (1 - start.Z, end.Y)
            };

            leftFace = TransformFace(leftFace, cube.Scale ?? Vector3.One, cube.Rotation ?? Vector3.Zero, cube.Translation ?? Vector3.Zero);

            int leftTextureIndex = GetTextureIndexFromCube(textures, cube, "Left");
            Vector3 leftNormal = CalculateNormalFromFace(leftFace);
            BlockModelCullDirection leftCullDirection = cube.Properties["Left"].CullDirection ?? noCullDirection[3] ?? DetermineCullDirection(leftNormal);

            ChunkVertex[] leftVertices =
            {

                new ChunkVertex(leftTextureIndex, leftFace[0], leftFaceTextureCoordinates[0], leftNormal, 0.0f, cube.Properties["Left"].ShouldRenderAO ?? true),
                new ChunkVertex(leftTextureIndex, leftFace[1], leftFaceTextureCoordinates[1], leftNormal, 0.0f, cube.Properties["Left"].ShouldRenderAO ?? true),
                new ChunkVertex(leftTextureIndex, leftFace[2], leftFaceTextureCoordinates[2], leftNormal, 0.0f, cube.Properties["Left"].ShouldRenderAO ?? true),
                new ChunkVertex(leftTextureIndex, leftFace[3], leftFaceTextureCoordinates[3], leftNormal, 0.0f, cube.Properties["Left"].ShouldRenderAO ?? true)

            };

            if (shouldFaceBeAdded[3])
            {

                if (!model.ChunkReadableFaces.ContainsKey(leftCullDirection))
                {

                    model.ChunkReadableFaces.Add(leftCullDirection, leftVertices);

                }
                else
                {

                    List<ChunkVertex> currentVertices = model.ChunkReadableFaces[leftCullDirection].ToList();
                    currentVertices.AddRange(leftVertices);
                    model.ChunkReadableFaces[leftCullDirection] = currentVertices.ToArray();

                }

            }

            Vector3[] backFace =
            {
                (start.X, end.Y, end.Z),
                (start.X, start.Y, end.Z),
                (end.X, start.Y, end.Z),
                (end.X, end.Y, end.Z)
            };

            Vector2[] backFaceTextureCoordinates =
            {
                (start.X, end.Y),
                (start.X, start.Y),
                (end.X, start.Y),
                (end.X, end.Y)
            };

            backFace = TransformFace(backFace, cube.Scale ?? Vector3.One, cube.Rotation ?? Vector3.Zero, cube.Translation ?? Vector3.Zero);

            int backTextureIndex = GetTextureIndexFromCube(textures, cube, "Back");
            Vector3 backNormal = CalculateNormalFromFace(backFace);
            BlockModelCullDirection backCullDirection = cube.Properties["Back"].CullDirection ?? noCullDirection[4] ?? DetermineCullDirection(backNormal);

            ChunkVertex[] backVertices =
            {

                new ChunkVertex(backTextureIndex, backFace[0], backFaceTextureCoordinates[0], backNormal, 0.0f, cube.Properties["Back"].ShouldRenderAO ?? true),
                new ChunkVertex(backTextureIndex, backFace[1], backFaceTextureCoordinates[1], backNormal, 0.0f, cube.Properties["Back"].ShouldRenderAO ?? true),
                new ChunkVertex(backTextureIndex, backFace[2], backFaceTextureCoordinates[2], backNormal, 0.0f, cube.Properties["Back"].ShouldRenderAO ?? true),
                new ChunkVertex(backTextureIndex, backFace[3], backFaceTextureCoordinates[3], backNormal, 0.0f, cube.Properties["Back"].ShouldRenderAO ?? true)

            };

            if (shouldFaceBeAdded[4])
            {

                if (!model.ChunkReadableFaces.ContainsKey(backCullDirection))
                {

                    model.ChunkReadableFaces.Add(backCullDirection, backVertices);

                }
                else
                {

                    List<ChunkVertex> currentVertices = model.ChunkReadableFaces[backCullDirection].ToList();
                    currentVertices.AddRange(backVertices);
                    model.ChunkReadableFaces[backCullDirection] = currentVertices.ToArray();

                }

            }

            Vector3[] rightFace =
            {

                (start.X, end.Y, start.Z),
                start,
                (start.X, start.Y, end.Z),
                (start.X, end.Y, end.Z)

            };

            Vector2[] rightFaceTextureCoordinates =
            {
                (start.Z, end.Y),
                (start.Z, start.Y),
                (end.Z, start.Y),
                (end.Z, end.Y)
            };

            rightFace = TransformFace(rightFace, cube.Scale ?? Vector3.One, cube.Rotation ?? Vector3.Zero, cube.Translation ?? Vector3.Zero);

            int rightTextureIndex = GetTextureIndexFromCube(textures, cube, "Right");
            Vector3 rightNormal = CalculateNormalFromFace(rightFace);
            BlockModelCullDirection rightCullDirection = cube.Properties["Right"].CullDirection ?? noCullDirection[5] ?? DetermineCullDirection(rightNormal);

            ChunkVertex[] rightVertices =
            {

                new ChunkVertex(rightTextureIndex, rightFace[0], rightFaceTextureCoordinates[0], rightNormal, 0.0f, cube.Properties["Right"].ShouldRenderAO ?? true),
                new ChunkVertex(rightTextureIndex, rightFace[1], rightFaceTextureCoordinates[1], rightNormal, 0.0f, cube.Properties["Right"].ShouldRenderAO ?? true),
                new ChunkVertex(rightTextureIndex, rightFace[2], rightFaceTextureCoordinates[2], rightNormal, 0.0f, cube.Properties["Right"].ShouldRenderAO ?? true),
                new ChunkVertex(rightTextureIndex, rightFace[3], rightFaceTextureCoordinates[3], rightNormal, 0.0f, cube.Properties["Right"].ShouldRenderAO ?? true)

            };

            if (shouldFaceBeAdded[5])
            {

                if (!model.ChunkReadableFaces.ContainsKey(rightCullDirection))
                {

                    model.ChunkReadableFaces.Add(rightCullDirection, rightVertices);

                }
                else
                {

                    List<ChunkVertex> currentVertices = model.ChunkReadableFaces[rightCullDirection].ToList();
                    currentVertices.AddRange(rightVertices);
                    model.ChunkReadableFaces[rightCullDirection] = currentVertices.ToArray();

                }

            }

        }
        public static void Parse(BlockModelAbstractData abstractModelData, BlockModel model)
        {

            if (abstractModelData.Inherit != null)
            {

                List<string> path = 
                [
                    
                    GlobalValues.BlockModelPath
                    
                ];
                path.AddRange(abstractModelData.Inherit.Split('/'));

                BlockModelAbstractData inheritModel = JsonSerializer.Deserialize<BlockModelAbstractData>(File.ReadAllText(Path.Combine(path.ToArray())), GlobalValues.DefaultJsonOptions);

                Debugger.Log($"The model has inherited model {abstractModelData.Inherit}", Severity.Info);

                for (int i = 0; i < inheritModel.Objects.Count(); i++)
                {

                    ParseCube(inheritModel.Objects[i], model, abstractModelData.Textures);

                }

            }

            if (abstractModelData.Objects.Count() != 0)
            {

                for (int i = 0; i < abstractModelData.Objects.Count(); i++)
                {

                    ParseCube(abstractModelData.Objects[i], model, abstractModelData.Textures);

                }

            }

        }

        public static BlockModel LoadFromJson(string fileName)
        {

            BlockModel model = new BlockModel();

            BlockModelAbstractData abstractModelData = JsonSerializer.Deserialize<BlockModelAbstractData>(File.ReadAllText(Path.Combine(GlobalValues.BlockModelPath, fileName)), GlobalValues.DefaultJsonOptions);

            Debugger.Log($"Parsing {fileName}", Severity.Info);
            Parse(abstractModelData, model);

            return model;

        }

        public static Vector3[] ApplyTransformations(Vector3[] originalPoints, Vector3 translation, Vector3 rotation, Vector3 scale)
        {

            Vector3[] facePoints = originalPoints;
            for (int i = 0; i < facePoints.Length; i++)
            {

                facePoints[i] /= 32.0f;
                facePoints[i] -= (0.5f, 0.5f, 0.5f);
                if (rotation != Vector3.Zero) facePoints[i] *= (Matrix3.CreateRotationX(Maths.ToRadians(rotation.X))
                                                             * Matrix3.CreateRotationY(Maths.ToRadians(rotation.Y))
                                                             * Matrix3.CreateRotationZ(Maths.ToRadians(rotation.Z)));
                if (scale != Vector3.One) facePoints[i] *= scale;
                if (translation != Vector3.Zero) facePoints[i] += translation;
                facePoints[i] += (0.5f, 0.5f, 0.5f);
                Console.WriteLine(facePoints[i]);

            }
            return facePoints;

        }

        public static Vector3 CalculateNormal(Vector3[] points)
        {

            Vector3[] transformedFace = points;
            Vector3 tangent = transformedFace[0] - transformedFace[1];
            Vector3 bitangent = transformedFace[1] - transformedFace[2];
            // Console.WriteLine($"tangent {tangent}, bitangent {bitangent}");
            Vector3 normal = Vector3.Cross(tangent, bitangent);
            // Console.WriteLine($"noraml {normal}");
            return normal;

        }

        public static Vector2[] CalculateTextureCoordinatesFromUvs(Vector2[] uvs)
        {

            Vector2[] newUvs = new Vector2[4];
            for (int i = 0; i < newUvs.Length; i++)
            {

                newUvs[i] = uvs[i];
                newUvs[i] /= 32.0f;

            }

            return newUvs;

        }
        public static Vector2[] CalculateTextureCoordinatesFromFace(Vector3[] points)
        {

            Vector2[] autoUvs = new Vector2[4];

            float yLength = (points[0] - points[1]).Length;
            float xLength = (points[1] - points[2]).Length;

            // Console.WriteLine($"{xLength}, {yLength}");

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
                Vector2[] textureCoordinates = CalculateTextureCoordinatesFromFace(points);
                if (face.Uvs != null) textureCoordinates = CalculateTextureCoordinatesFromUvs(face.Uvs);
                if (inheritedFaceData?.Uvs != null) textureCoordinates = CalculateTextureCoordinatesFromUvs(inheritedFaceData?.Uvs);
                // Vector2[] textureCoordinates = face.Uvs ?? inheritedFaceData?.Uvs ?? CalculateTextureCoordinatesFromFace(points);
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

                    // Console.WriteLine($"{parsedVertices[i].Position}, {parsedVertices[i].Normal}, {parsedVertices[i].TextureCoordinates}, {parsedVertices[i].TextureIndex}");

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

        public void AddRotation(Vector3 rotation)
        {

            if (ChunkReadableFaces.ContainsKey(BlockModelCullDirection.None))
            {

                for (int i = 0; i < ChunkReadableFaces[BlockModelCullDirection.None].Length; i++)
                {

                    Vector3 position = ChunkReadableFaces[BlockModelCullDirection.None][i].Position;
                    if (rotation.X != 0) position *= Matrix3.CreateRotationX(Maths.ToRadians(rotation.X));
                    if (rotation.Y != 0) position *= Matrix3.CreateRotationY(Maths.ToRadians(rotation.Y));
                    if (rotation.Z != 0) position *= Matrix3.CreateRotationZ(Maths.ToRadians(rotation.Z));

                }

            }

        }

        public float[] DetermineAmbientValues(ConcurrentDictionary<Vector3i, Chunk> worldChunks, Vector3i chunkPosition, BlockModelCullDirection direction, Vector3i blockPosition)
        {

            int amountSolid = 0;
            for (int x = -1; x <= 1; x++)
            {

                for (int  y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {

                        if ((x,y,z) != Vector3i.Zero)
                        {

                            if (worldChunks[ChunkUtils.PositionToChunk(blockPosition + (x, y, z))].SolidMask[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(blockPosition + (x, y, z)))]) amountSolid++;

                        }

                    }

                }

            }

            float[] ambientValues = new float[4];
            if (amountSolid == 26) return ambientValues;

            if (worldChunks[chunkPosition].GetBlock(ChunkUtils.PositionToBlockLocal(blockPosition)).RenderAo ?? true)
            {

                Vector3i[] sampleDirections = new Vector3i[9];
                switch (direction)
                {

                    case BlockModelCullDirection.Up:
                        sampleDirections = [(1, 1, 1), (1, 1, 0), (1, 1, -1), (0, 1, 1), (0, 1, 0), (0, 1, -1), (-1, 1, 1), (-1, 1, 0), (-1, 1, -1)];
                        break;
                    case BlockModelCullDirection.Down:
                        sampleDirections = [(-1, -1, 1), (-1, -1, 0), (-1, -1, -1), (0, -1, 1), (0, -1, 0), (0, -1, -1), (1, -1, 1), (1, -1, 0), (1, -1, -1)];
                        break;
                    case BlockModelCullDirection.Left:
                        sampleDirections = [(1, 1, 1), (1, 0, 1), (1, -1, 1), (1, 1, 0), (1, 0, 0), (1, -1, 0), (1, 1, -1), (1, 0, -1), (1, -1, -1)];
                        break;
                    case BlockModelCullDirection.Right:
                        sampleDirections = [(-1, 1, -1), (-1, 0, -1), (-1, -1, -1), (-1, 1, 0), (-1, 0, 0), (-1, -1, 0), (-1, 1, 1), (-1, 0, 1), (-1, -1, 1)];
                        break;
                    case BlockModelCullDirection.Back:
                        sampleDirections = [(-1, 1, 1), (-1, 0, 1), (-1, -1, 1), (0, 1, 1), (0, 0, 1), (0, -1, 1), (1, 1, 1), (1, 0, 1), (1, -1, 1)];
                        break;
                    case BlockModelCullDirection.Front:
                        sampleDirections = [(1, 1, -1), (1, 0, -1), (1, -1, -1), (0, 1, -1), (0, 0, -1), (0, -1, -1), (-1, 1, -1), (-1, 0, -1), (-1, -1, -1)];
                        break;

                }

                bool[] ambientBools = new bool[9];
                for (int i = 0; i < ambientBools.Length; i++)
                {

                    // if (worldChunks[ChunkUtils.PositionToChunk(blockPosition + sampleDirections[i])].GetBlock(ChunkUtils.PositionToBlockLocal(blockPosition + sampleDirections[i])) != Blocks.AirBlock) ambientBools[i] = true;

                    if (worldChunks[ChunkUtils.PositionToChunk(blockPosition + sampleDirections[i])].SolidMask[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(blockPosition + sampleDirections[i]))]) ambientBools[i] = true;

                }

                if (ambientBools[0]) ambientValues[0]++;
                if (ambientBools[1]) { ambientValues[0]++; ambientValues[1]++; }
                if (ambientBools[2]) { ambientValues[1]++; }
                if (ambientBools[3]) { ambientValues[0]++; ambientValues[3]++; }
                if (ambientBools[4]) { ambientValues[0]++; ambientValues[1]++; ambientValues[2]++; ambientValues[3]++; }
                if (ambientBools[5]) { ambientValues[1]++; ambientValues[2]++; }
                if (ambientBools[6]) ambientValues[3]++;
                if (ambientBools[7]) { ambientValues[2]++; ambientValues[3]++; }
                if (ambientBools[8]) ambientValues[2]++;

            }

            return ambientValues;

        }

        public ChunkVertex[] GetOffsettedFace(BlockModelCullDirection direction, Vector3i localOffset, Dictionary<Vector3i, bool[]> mask)
        {

            ChunkVertex[] vertices = new ChunkVertex[ChunkReadableFaces[direction].Length];
            for (int i = 0; i < vertices.Length; i++)
            {

                vertices[i] = ChunkReadableFaces[direction][i];
                vertices[i].Position += localOffset;

            }

            for (int i = 0; i < vertices.Length; i+=4)
            {

                if (vertices[i].ShouldRenderAO)
                {

                    Vector3 normal = vertices[i].Normal;
                    if (GlobalValues.Toggle) Console.WriteLine(normal);

                    Vector3i[] samplePoints = Array.Empty<Vector3i>();

                    switch (normal)
                    {

                        case (0, 1, 0):
                            samplePoints = [(1, 1, 1), (1, 1, 0), (1, 1, -1), (0, 1, 1), (0, 1, 0), (0, 1, -1), (-1, 1, 1), (-1, 1, 0), (-1, 1, -1)];
                            break;
                        case (0, -1, 0):
                            samplePoints = [(1, -1, -1), (1, -1, 0), (1, -1, 1), (0, -1, -1), (0, -1, 0), (0, -1, 1), (1, -1, -1), (1, -1, 0), (1, -1, 1)];
                            break;
                        case (1, 0, 0):
                            samplePoints = [(1, 1, 1), (1, 0, 1), (1, -1, 1), (1, 1, 0), (1, 0, 0), (1, -1, 0), (1, 1, -1), (1, 0, -1), (1, -1, -1)];
                            break;
                        case (-1, 0, 0):
                            samplePoints = [(-1, 1, -1), (-1, 0, -1), (-1, -1, -1), (-1, 1, 0), (-1, 0, 0), (-1, -1, 0), (-1, 1, 1), (-1, 0, 1), (-1, -1, 1)];
                            break;
                        case (0, 0, -1):
                            samplePoints = [(1, 1, -1), (1, 0, -1), (1, -1, -1), (0, 1, -1), (0, 0, -1), (0, -1, -1), (-1, 1, -1), (-1, 0, -1), (-1, -1, -1)];
                            break;
                        case (0, 0, 1):
                            samplePoints = [(-1, 1, 1), (-1, 0, 1), (-1, -1, 1), (0, 1, 1), (0, 0, 1), (0, -1, 1), (1, 1, 1), (1, 0, 1), (1, -1, 1)];
                            break;

                    }

                    bool[] ambientMask = new bool[9];
                    if (samplePoints.Length != 0)
                    {

                        for (int a = 0; a < samplePoints.Length; a++)
                        {

                            if (mask[ChunkUtils.PositionToChunk(localOffset + samplePoints[a])][ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(localOffset + samplePoints[a]))]) ambientMask[a] = true;

                        }

                    }

                    if (ambientMask[0]) vertices[0 + i].AmbientValue++;
                    if (ambientMask[1]) { vertices[0 + i].AmbientValue++; vertices[1 + i].AmbientValue++; }
                    if (ambientMask[2]) vertices[1 + i].AmbientValue++;
                    if (ambientMask[3]) { vertices[0 + i].AmbientValue++; vertices[3 + i].AmbientValue++; }
                    if (ambientMask[4]) { vertices[0 + i].AmbientValue++; vertices[1 + i].AmbientValue++; vertices[2 + i].AmbientValue++; vertices[3 + i].AmbientValue++; }
                    if (ambientMask[5]) { vertices[1 + i].AmbientValue++; vertices[2 + i].AmbientValue++; }
                    if (ambientMask[6]) vertices[3 + i].AmbientValue++;
                    if (ambientMask[7]) { vertices[2 + i].AmbientValue++; vertices[3 + i].AmbientValue++; }
                    if (ambientMask[8]) vertices[2 + i].AmbientValue++;

                }

            }

            return vertices;

        }

        public ChunkVertex[] GetFaceWithOffsetAO(ConcurrentDictionary<Vector3i, Chunk> worldChunks, Vector3i chunkPosition, BlockModelCullDirection direction, Vector3i offset)
        {

            ChunkVertex[] vertices = new ChunkVertex[ChunkReadableFaces[direction].Length];
            Vector3i globalBlockPosition = offset + (chunkPosition * GlobalValues.ChunkSize);
            for (int i = 0; i < vertices.Length; i++)
            {

                vertices[i] = ChunkReadableFaces[direction][i];
                vertices[i].Position += offset;

            }

            float[] ambientValues = DetermineAmbientValues(worldChunks, chunkPosition, direction, offset + (chunkPosition * 32));
            for (int i = 0; i < vertices.Length; i++)
            {

                switch (vertices[i].Normal)
                {

                    case (0, 1, 0):
                        vertices[i].LightData = worldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + Vector3i.UnitY)].PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition + Vector3i.UnitY))];
                        break;
                    case (0, -1, 0):
                        vertices[i].LightData = worldChunks[ChunkUtils.PositionToChunk(globalBlockPosition - Vector3i.UnitY)].PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition - Vector3i.UnitY))];
                        break;
                    case (1, 0, 0):
                        vertices[i].LightData = worldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + Vector3i.UnitX)].PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition + Vector3i.UnitX))];
                        break;
                    case (-1, 0, 0):
                        vertices[i].LightData = worldChunks[ChunkUtils.PositionToChunk(globalBlockPosition - Vector3i.UnitX)].PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition - Vector3i.UnitX))];
                        break;
                    case (0, 0, 1):
                        vertices[i].LightData = worldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + Vector3i.UnitZ)].PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition + Vector3i.UnitZ))];
                        break;
                    case (0, 0, -1):
                        vertices[i].LightData = worldChunks[ChunkUtils.PositionToChunk(globalBlockPosition - Vector3i.UnitZ)].PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition - Vector3i.UnitZ))];
                        break;
                    default:
                        vertices[i].LightData = worldChunks[chunkPosition].PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(offset))];
                        break;

                }

            }

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

                int currentCount = (worldChunks[chunkPosition].ConcurrentMeshIndices.Count / 6) * 4;
                int[] indices =
                {

                0+currentCount,1+currentCount,2+currentCount,2+currentCount,3+currentCount,0+currentCount

            };
                worldChunks[chunkPosition].AddIndices(indices);

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
