using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using Game.Core.Chunks;
using Game.Core.TexturePack;
using Game.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Tomlet;
using Tomlet.Attributes;
using Tomlet.Models;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace Game.BlockUtil;

public enum Direction : byte
{
    
    None = 0b000000,
    Top = 0b000001,
    Bottom = 0b000010,
    Left = 0b000100,
    Right = 0b001000,
    Back = 0b010000,
    Front = 0b100000
    
}

[StructLayout(LayoutKind.Explicit, Size = 144)]
public struct Rectangle
{
    [FieldOffset(0)]
    public Vector4 LightTopLeft;
    [FieldOffset(16)]
    public Vector4 LightBottomLeft;
    [FieldOffset(32)]
    public Vector4 LightBottomRight;
    [FieldOffset(48)]
    public Vector4 LightTopRight;
    [FieldOffset(64)]
    public Vector3 Position;
    [FieldOffset(80)]
    public Vector3 Tangent;
    [FieldOffset(96)]
    public Vector3 Bitangent;
    [FieldOffset(112)]
    public Vector2 TextureCoordinateDimensions;
    [FieldOffset(120)]
    public Vector2 TextureCoordinateOffset;
    [FieldOffset(128)]
    public Vector2 Size;
    [FieldOffset(136)]
    public uint TextureIndex;
}

public struct FaceProperties
{
    
    [TomlProperty("texture_name")] public string TextureName { get; set; }
    [TomlProperty("should_render_ao")] public bool? ShouldRenderAo { get; set; }
    [TomlProperty("is_visible")] public bool? IsVisible { get; set; }

    public FaceProperties() {}

}

public struct Cube
{
    [TomlProperty("start")] public Vector3 Start { get; set; }
    [TomlProperty("end")] public Vector3 End { get; set; }

    [TomlProperty("rotation")] public Vector3 Rotation { get; set; }
    [TomlProperty("origin")] public Vector3 Origin { get; set; }
    
    [TomlProperty("properties")] public Dictionary<string, FaceProperties> Properties { get; set; }

    public string TopTextureName;
    public string BottomTextureName;
    public string LeftTextureName;
    public string RightTextureName;
    public string BackTextureName;
    public string FrontTextureName;

    public Cube() { }
    
    private byte _visibleFlags = 0b111111;

    public Cube(Vector3 start, Vector3 end)
    {
        Start = start;
        End = end;
    }

    public void SetVisible(Direction direction, bool isVisible)
    {

        if (isVisible)
        {
            _visibleFlags = (byte) (_visibleFlags | (byte)direction);
        }
        else
        {
            _visibleFlags = (byte) (_visibleFlags & ~(byte)direction);
        }

    }

    public bool IsVisible(Direction direction)
    {
        
        return (_visibleFlags & (byte)direction) != 0;

    }

}

public class PrimitiveModelData
{

    [TomlProperty("inherit")] public string InheritPath { get; set; }
    [TomlProperty("textures")] public Dictionary<string, string> Textures { get; set; }
    [TomlProperty("cubes")] public List<Cube> Cubes { get; set; }

}

public class BlockModel
{
    
    private Dictionary<Direction, List<PackedChunkVertex>> _computedModelPackedVertices = new();
    private Dictionary<Direction, List<ChunkVertex>> _computedModelVertices = new();
    private Dictionary<Direction, List<Rectangle>> _solidModelFaces = new();
    // private Dictionary<Direction, List<CustomVertex>> _computedModelGeneralVertices = new();
    public bool IsFullBlock = true;

    // FIXME: remove this shit
    public List<PackedChunkVertex> QueryPackedFace(Direction direction, Vector3i offset, LightColor lightColor)
    {
        List<PackedChunkVertex> result = [];
        if (_computedModelPackedVertices.TryGetValue(direction, out List<PackedChunkVertex> packedVertices))
        {
            for (int i = 0; i < packedVertices.Count; i++)
            {
                PackedChunkVertex vertex = packedVertices[i];
                vertex.Position += offset;
                vertex.LightColor = Vector3.One;
                result.Add(vertex);
            }
        }
        return result;

    }

    public void QueryFace(List<Rectangle> rectangles, Direction direction, Vector3i offset)
    {

        if (_solidModelFaces.ContainsKey(direction))
        {
            foreach (Rectangle rectangle in _solidModelFaces[direction])
            {
                Rectangle rect = rectangle;
                rect.Position += offset;
                rectangles.Add(rect);
            }
        }

    }

    public static BlockModel FromCubes(List<Cube> cubes)
    {
        
        BlockModel blockModel = new BlockModel();
        
        return blockModel;

    }

    public static BlockModel FromToml(string tomlPath)
    {

        Console.WriteLine(tomlPath);

        PrimitiveModelData modelData = TomletMain.To<PrimitiveModelData>(File.ReadAllText(Path.Combine(GlobalValues.BlockModelPath, tomlPath)));
        if (modelData.InheritPath != null)
        {
            string inheritPath = Path.Combine(GlobalValues.BlockModelPath, Path.Combine(modelData.InheritPath.Split('/')));
            if (modelData.Cubes == null) modelData.Cubes = [];
            CheckInheritCubes(modelData.Cubes, inheritPath);
        }
        
        return Parse(modelData);
    }

    private static void CheckInheritCubes(List<Cube> cubes, string inheritPath) {

        PrimitiveModelData inheritData = TomletMain.To<PrimitiveModelData>(File.ReadAllText(inheritPath));
        cubes.AddRange(inheritData.Cubes);
        if (inheritData.InheritPath != null) {
            CheckInheritCubes(cubes, Path.Combine(GlobalValues.BlockModelPath, Path.Combine(inheritData.InheritPath.Split('/'))));
        } 

    }

    private static BlockModel Parse(PrimitiveModelData modelData)
    {

        BlockModel model = new BlockModel();

        foreach (Cube cube in modelData.Cubes)
        {

            if (model._solidModelFaces.ContainsKey(Direction.Top))
            {
                model._solidModelFaces[Direction.Top].Add(new Rectangle() { 
                    Position = new Vector3(cube.Start.X, cube.End.Y, cube.Start.Z) / 16.0f,
                    Size = (cube.End.Xz - cube.Start.Xz) / 16.0f,
                    Tangent = (0, 0, 1), 
                    Bitangent = (1, 0, 0),
                    TextureCoordinateOffset = cube.Start.Xz / 16.0f,
                    TextureCoordinateDimensions = (cube.End.Xz - cube.Start.Xz) / 16.0f,
                    TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["top"].TextureName])
                });
            } else
            {
                model._solidModelFaces.Add(Direction.Top, [new Rectangle() { 
                    Position = new Vector3(cube.Start.X, cube.End.Y, cube.Start.Z) / 16.0f, 
                    Size = (cube.End.Xz - cube.Start.Xz) / 16.0f,
                    Tangent = (0, 0, 1), 
                    Bitangent = (1, 0, 0), 
                    TextureCoordinateOffset = cube.Start.Xz / 16.0f, 
                    TextureCoordinateDimensions = (cube.End.Xz - cube.Start.Xz) / 16.0f,
                    TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["top"].TextureName])
                }]);
            }

            if (model._solidModelFaces.ContainsKey(Direction.Front))
            {
                model._solidModelFaces[Direction.Front].Add(new Rectangle() {
                    Position = cube.Start / 15.0f,
                    Size = (cube.End.Xy - cube.Start.Xy) / 16.0f,
                    Tangent = (0, 1, 0),
                    Bitangent = (1, 0, 0),
                    TextureCoordinateOffset = cube.Start.Xy / 16.0f,
                    TextureCoordinateDimensions = (cube.End.Xy - cube.Start.Xy) / 16.0f,
                    TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["front"].TextureName])
                });
            } else
            {
                model._solidModelFaces.Add(Direction.Front, [new Rectangle() {
                    Position = cube.Start / 16.0f,
                    Size = (cube.End.Xy - cube.Start.Xy) / 16.0f,
                    Tangent = (0, 1, 0),
                    Bitangent = (1, 0, 0),
                    TextureCoordinateOffset = cube.Start.Xy / 16.0f,
                    TextureCoordinateDimensions = (cube.End.Xy - cube.Start.Xy) / 16.0f,
                    TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["front"].TextureName])
                }]);
            }

            if (model._solidModelFaces.ContainsKey(Direction.Left))
            {
                model._solidModelFaces[Direction.Left].Add(new Rectangle() {
                    Position = new Vector3(cube.End.X, cube.Start.Y, cube.Start.Z) / 16.0f,
                    Size = (cube.End.Zy - cube.Start.Zy) / 16.0f,
                    Tangent = (0, 1, 0),
                    Bitangent = (0, 0, 1),
                    TextureCoordinateOffset = cube.Start.Zy / 16.0f,
                    TextureCoordinateDimensions = (cube.End.Zy - cube.Start.Zy) / 16.0f,
                    TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["left"].TextureName])
                });
            } else
            {
                model._solidModelFaces.Add(Direction.Left, [new Rectangle() {
                    Position = new Vector3(cube.End.X, cube.Start.Y, cube.Start.Z) / 16.0f,
                    Size = (cube.End.Zy - cube.Start.Zy) / 16.0f,
                    Tangent = (0, 1, 0),
                    Bitangent = (0, 0, 1),
                    TextureCoordinateOffset = cube.Start.Zy / 16.0f,
                    TextureCoordinateDimensions = (cube.End.Zy - cube.Start.Zy) / 16.0f,
                    TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["left"].TextureName])
                }]);
            }

            if (model._solidModelFaces.ContainsKey(Direction.Back))
            {
                model._solidModelFaces[Direction.Back].Add(new Rectangle() {
                    Position = new Vector3(cube.End.X, cube.Start.Y, cube.End.Z) / 16.0f,
                    Size = (cube.End.Xy - cube.Start.Xy) / 16.0f,
                    Tangent = (0, 1, 0),
                    Bitangent = (-1, 0, 0),
                    TextureCoordinateOffset = cube.Start.Xy / 16.0f,
                    TextureCoordinateDimensions = (cube.End.Xy - cube.Start.Xy) / 16.0f,
                    TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["back"].TextureName])
                });
            } else 
            {
                model._solidModelFaces.Add(Direction.Back, [new Rectangle() {
                    Position = new Vector3(cube.End.X, cube.Start.Y, cube.End.Z) / 16.0f,
                    Size = (cube.End.Xy - cube.Start.Xy) / 16.0f,
                    Tangent = (0, 1, 0),
                    Bitangent = (-1, 0, 0),
                    TextureCoordinateOffset = cube.Start.Xy / 16.0f,
                    TextureCoordinateDimensions = (cube.End.Xy - cube.Start.Xy) / 16.0f,
                    TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["back"].TextureName])
                }]);
            }

            if (model._solidModelFaces.ContainsKey(Direction.Right))
            {
                model._solidModelFaces[Direction.Right].Add(new Rectangle() {
                    Position = new Vector3(cube.Start.X, cube.Start.Y, cube.End.Z) / 16.0f,
                    Size = (cube.End.Zy - cube.Start.Zy) / 16.0f,
                    Tangent = (0, 1, 0),
                    Bitangent = (0, 0, -1),
                    TextureCoordinateOffset = cube.Start.Zy / 16.0f,
                    TextureCoordinateDimensions = (cube.End.Zy - cube.Start.Zy) / 16.0f,
                    TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["right"].TextureName])
                });
            } else
            {
                model._solidModelFaces.Add(Direction.Right, [new Rectangle() {
                    Position = new Vector3(cube.Start.X, cube.Start.Y, cube.End.Z) / 16.0f,
                    Size = (cube.End.Zy - cube.Start.Zy) / 16.0f,
                    Tangent = (0, 1, 0),
                    Bitangent = (0, 0, -1),
                    TextureCoordinateOffset = cube.Start.Zy / 16.0f,
                    TextureCoordinateDimensions = (cube.End.Zy - cube.Start.Zy) / 16.0f,
                    TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["right"].TextureName])
                }]);
            }

            if (model._solidModelFaces.ContainsKey(Direction.Bottom))
            {
                model._solidModelFaces[Direction.Bottom].Add(new Rectangle() {
                    Position = new Vector3(cube.Start.X, cube.Start.Y, cube.End.Z) / 16.0f,
                    Size = (cube.End.Xz - cube.Start.Xz) / 16.0f,
                    Tangent = (0, 0, -1),
                    Bitangent = (1, 0, 0),
                    TextureCoordinateOffset = cube.Start.Xz / 16.0f,
                    TextureCoordinateDimensions = (cube.End.Xz - cube.Start.Xz) / 16.0f,
                    TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["bottom"].TextureName])
                });
            } else
            {
                model._solidModelFaces.Add(Direction.Bottom, [new Rectangle() {
                    Position = new Vector3(cube.Start.X, cube.Start.Y, cube.End.Z) / 16.0f,
                    Size = (cube.End.Xz - cube.Start.Xz) / 16.0f,
                    Tangent = (0, 0, -1),
                    Bitangent = (1, 0, 0),
                    TextureCoordinateOffset = cube.Start.Xz / 16.0f,
                    TextureCoordinateDimensions = (cube.End.Xz - cube.Start.Xz) / 16.0f,
                    TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["bottom"].TextureName])
                }]);
            }

            /*
            // can go in packed dictionary
            if (cube.Start == (0, 0, 0) && cube.End == (32, 32, 32) && 
                cube.Rotation == (0, 0, 0) && cube.Origin == (0, 0, 0))
            {

                Vector3i normalizedEnd = (Vector3i) (cube.End / 32.0f);
                Vector3i normalizedStart = (Vector3i) (cube.Start / 32.0f);
                foreach (KeyValuePair<string, FaceProperties> faceProperties in cube.Properties)
                {

                    if (faceProperties.Value.IsVisible ?? true)
                    {

                        switch (faceProperties.Key)
                        {
                            case "top":
                                // Console.WriteLine($"{modelData.Textures[faceProperties.Value.TextureName]}, {TexturePackManager.GetTextureHandleIndex(modelData.Textures[faceProperties.Value.TextureName])}");
                                model._computedModelPackedVertices.Add(Direction.Top, [
                                    new PackedChunkVertex(normalizedEnd, Core.Chunks.Direction.Up, 0, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedEnd - (0, 0, 1),Core.Chunks.Direction.Up, 1, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (0, 1, 0), Core.Chunks.Direction.Up, 2, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (0, 1, 1), Core.Chunks.Direction.Up, 3, modelData.Textures[faceProperties.Value.TextureName]),
                                ]);
                                break;
                            case "bottom":
                                model._computedModelPackedVertices.Add(Direction.Bottom, [
                                    new PackedChunkVertex(normalizedStart + (1, 0, 0), Core.Chunks.Direction.Down, 0, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedEnd - (0, 1, 0), Core.Chunks.Direction.Down, 1, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (0, 0, 1), Core.Chunks.Direction.Down, 2, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart, Core.Chunks.Direction.Down, 3, modelData.Textures[faceProperties.Value.TextureName])
                                ]);
                                break;
                            case "right":
                                model._computedModelPackedVertices.Add(Direction.Right, [
                                    new PackedChunkVertex(normalizedStart + (0, 1, 0), Core.Chunks.Direction.Right, 0, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart, Core.Chunks.Direction.Right, 1, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (0, 0, 1), Core.Chunks.Direction.Right, 2, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (0, 1, 1), Core.Chunks.Direction.Right, 3, modelData.Textures[faceProperties.Value.TextureName])
                                ]);
                                break;
                            case "left":
                                model._computedModelPackedVertices.Add(Direction.Left, [
                                    new PackedChunkVertex(normalizedEnd, Core.Chunks.Direction.Left, 0, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedEnd - (0, 1, 0), Core.Chunks.Direction.Left, 1, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (1, 0, 0), Core.Chunks.Direction.Left, 2, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedEnd - (0, 0, 1), Core.Chunks.Direction.Left, 3, modelData.Textures[faceProperties.Value.TextureName])
                                ]);
                                break;
                            case "back":
                                model._computedModelPackedVertices.Add(Direction.Back, [
                                    new PackedChunkVertex(normalizedStart + (0, 1, 1), Core.Chunks.Direction.Back, 0, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (0, 0, 1), Core.Chunks.Direction.Back, 1, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedEnd - (0, 1, 0), Core.Chunks.Direction.Back, 2, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedEnd, Core.Chunks.Direction.Back, 3, modelData.Textures[faceProperties.Value.TextureName])
                                ]);
                                break;
                            case "front":
                                model._computedModelPackedVertices.Add(Direction.Front, [
                                    new PackedChunkVertex(normalizedStart + (1, 1, 0), Core.Chunks.Direction.Front, 0, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (1, 0, 0), Core.Chunks.Direction.Front, 1, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart, Core.Chunks.Direction.Front, 2, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (0, 1, 0), Core.Chunks.Direction.Front, 3, modelData.Textures[faceProperties.Value.TextureName])
                                ]);
                                break;
                            
                        }
                        
                    }
                    
                }
                
            }
            else
            {
                
                
                
            }
            */
            
        }
        
        return model;

    }

}