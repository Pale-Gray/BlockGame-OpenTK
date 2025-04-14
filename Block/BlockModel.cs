using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Game.Core.Chunks;
using Game.Core.TexturePack;
using Game.Util;
using LiteNetLib.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.Wgl;
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
    [TomlProperty("should_render_ao")] public bool ShouldRenderAo { get; set; } = true;
    [TomlProperty("is_visible")] public bool IsVisible { get; set; } = true;
    [TomlProperty("cull_direction")] public Direction? CullDirection { get; set; } = null;

    public FaceProperties() {}

}

public struct Cube
{
    [TomlProperty("start")] public Vector3 Start { get; set; }
    [TomlProperty("end")] public Vector3 End { get; set; }

    [TomlProperty("rotation")] public Vector3 Rotation { get; set; }
    [TomlProperty("origin")] public Vector3 Origin { get; set; } = (8, 8, 8);
    
    [TomlProperty("properties")] public Dictionary<string, FaceProperties> Properties { get; set; }

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
    
    private Dictionary<Direction, List<Rectangle>> _solidModelFaces = new();
    private Dictionary<Direction, List<Rectangle>> _cutoutModelFaces = new();
    private Dictionary<Direction, List<Rectangle>> _freeformModelFaces = new();
    public bool IsFullBlock = true;

    public BlockModel()
    {

        _solidModelFaces = new()
        {
            {Direction.Top, []},
            {Direction.Bottom, []},
            {Direction.Front, []},
            {Direction.Left, []},
            {Direction.Back, []},
            {Direction.Right, []}
        };  

        _cutoutModelFaces = new()
        {
            {Direction.None, []},
            {Direction.Top, []},
            {Direction.Bottom, []},
            {Direction.Front, []},
            {Direction.Left, []},
            {Direction.Back, []},
            {Direction.Right, []}
        };

        _freeformModelFaces = new()
        {
            {Direction.None, []},
            {Direction.Top, []},
            {Direction.Bottom, []},
            {Direction.Front, []},
            {Direction.Left, []},
            {Direction.Back, []},
            {Direction.Right, []}
        };

    }
    public void AddAmbientOcclusionFace(List<Rectangle> rectangles, Direction direction, Vector3i offset, (Vector4, Vector4, Vector4, Vector4) lightData)
    {

        if (_solidModelFaces.ContainsKey(direction))
        {
            foreach (Rectangle rectangle in _solidModelFaces[direction])
            {
                Rectangle rect = rectangle;
                rect.Position += offset;
                rect.LightTopLeft = lightData.Item1;
                rect.LightBottomLeft = lightData.Item2;
                rect.LightBottomRight = lightData.Item3;
                rect.LightTopRight = lightData.Item4;
                rectangles.Add(rect);
            }
        }

    }

    public void AddFreeformFace(List<Rectangle> rectangles, Direction direction, Vector3i offset, Vector4 lightData)
    {

        if (_freeformModelFaces.ContainsKey(direction))
        {

            foreach (Rectangle rectangle in _freeformModelFaces[direction])
            {

                Rectangle rect = rectangle;
                rect.Position += offset;
                rect.LightTopLeft = lightData;
                rect.LightBottomLeft = lightData;
                rect.LightBottomRight = lightData;
                rect.LightTopRight = lightData;
                rectangles.Add(rect);

            }

        }

    }

    public void AddCutoutFace(List<Rectangle> rectangles, Direction direction, Vector3i offset, Vector4 lightData)
    {

        if (_cutoutModelFaces.ContainsKey(direction))
        {

            foreach (Rectangle rectangle in _cutoutModelFaces[direction])
            {

                Rectangle rect = rectangle;
                rect.Position += offset;
                rect.LightTopLeft = lightData;
                rect.LightBottomLeft = lightData;
                rect.LightBottomRight = lightData;
                rect.LightTopRight = lightData;
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

            if (cube.Start.Y == cube.End.Y && cube.Properties["top"].IsVisible)
            {
                
                Rectangle rect = new Rectangle();
                rect.Position = new Vector3(cube.Start.X, cube.End.Y, cube.Start.Z) / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Xz - cube.Start.Xz) / 16.0f;
                rect.Tangent = new Vector3(0, 0, cube.End.Z - cube.Start.Z).Normalized();
                // rect.Tangent = (0, 0, 1);
                rect.Bitangent = new Vector3(cube.End.X - cube.Start.X, 0, 0).Normalized();
                // rect.Bitangent = (1, 0, 0);
                rect.TextureCoordinateOffset = cube.Start.Xz / 16.0f;
                rect.TextureCoordinateDimensions = rect.Size;
                rect.TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["top"].TextureName]);

                if (cube.Rotation != Vector3.Zero)
                {

                    Matrix3 rotation = Matrix3.CreateRotationZ(Maths.ToRadians(cube.Rotation.Z))
                                     * Matrix3.CreateRotationY(Maths.ToRadians(cube.Rotation.Y))
                                     * Matrix3.CreateRotationX(Maths.ToRadians(cube.Rotation.X));

                    rect.Position -= cube.Origin / 16.0f;
                    rect.Position *= rotation;
                    rect.Tangent *= rotation;
                    rect.Bitangent *= rotation;
                    rect.Position += cube.Origin / 16.0f;

                    model._cutoutModelFaces[cube.Properties["top"].CullDirection ?? ResolveDirection(rect)].Add(rect);

                } else
                {

                    model._cutoutModelFaces[cube.Properties["top"].CullDirection ?? Direction.Top].Add(rect);

                }
                continue;
                
            }

            if (cube.Start.X == cube.End.X && cube.Properties["left"].IsVisible)
            {

                Rectangle rect = new Rectangle();
                rect.Position = new Vector3(cube.Start.X, cube.Start.Y, cube.Start.Z) / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Zy - cube.Start.Zy) / 16.0f;
                rect.Tangent = new Vector3(0, cube.End.Y - cube.Start.Y, 0).Normalized();
                // rect.Tangent = (0, 1, 0);
                rect.Bitangent = new Vector3(0, 0, cube.End.Z - cube.Start.Z).Normalized();
                // rect.Bitangent = (0, 0, 1);
                rect.TextureCoordinateOffset = cube.Start.Zy / 16.0f;
                rect.TextureCoordinateDimensions = rect.Size;
                rect.TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["left"].TextureName]);

                if (cube.Rotation != Vector3.Zero)
                {

                    Matrix3 rotation = Matrix3.CreateRotationZ(Maths.ToRadians(cube.Rotation.Z))
                                     * Matrix3.CreateRotationY(Maths.ToRadians(cube.Rotation.Y))
                                     * Matrix3.CreateRotationX(Maths.ToRadians(cube.Rotation.X));

                    rect.Position -= cube.Origin / 16.0f;
                    rect.Position *= rotation;
                    rect.Tangent *= rotation;
                    rect.Bitangent *= rotation;
                    rect.Position += cube.Origin / 16.0f;

                    model._cutoutModelFaces[cube.Properties["left"].CullDirection ?? ResolveDirection(rect)].Add(rect);

                } else
                {

                    model._cutoutModelFaces[cube.Properties["left"].CullDirection ?? Direction.Left].Add(rect);

                }
                continue;

            }

            if (cube.Start.Z == cube.End.Z && cube.Properties["front"].IsVisible)
            {

                Rectangle rect = new Rectangle();
                rect.Position = new Vector3(cube.Start.X, cube.Start.Y, cube.Start.Z) / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Xy - cube.Start.Xy) / 16.0f;
                rect.Tangent = new Vector3(0, cube.End.Y - cube.Start.Y, 0).Normalized();
                // rect.Tangent = (0, 1, 0);
                rect.Bitangent = new Vector3(cube.End.X - cube.Start.X, 0, 0).Normalized();
                // rect.Bitangent = (-1, 0, 0);
                rect.TextureCoordinateOffset = cube.Start.Xy / 16.0f;
                rect.TextureCoordinateDimensions = rect.Size;
                rect.TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["front"].TextureName]);

                if (cube.Rotation != Vector3.Zero)
                {

                    Matrix3 rotation = Matrix3.CreateRotationZ(Maths.ToRadians(cube.Rotation.Z))
                                     * Matrix3.CreateRotationY(Maths.ToRadians(cube.Rotation.Y))
                                     * Matrix3.CreateRotationX(Maths.ToRadians(cube.Rotation.X));

                    rect.Position -= cube.Origin / 16.0f;
                    rect.Position *= rotation;
                    rect.Tangent *= rotation;
                    rect.Bitangent *= rotation;
                    rect.Position += cube.Origin / 16.0f;

                    model._cutoutModelFaces[cube.Properties["front"].CullDirection ?? ResolveDirection(rect)].Add(rect);

                } else
                {

                    model._cutoutModelFaces[cube.Properties["front"].CullDirection ?? Direction.Front].Add(rect);

                }
                continue;
                
            }

            if (cube.Properties["top"].IsVisible)
            {

                Rectangle rect = new Rectangle();
                rect.Position = new Vector3(cube.Start.X, cube.End.Y, cube.Start.Z) / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Xz - cube.Start.Xz) / 16.0f;
                rect.Tangent = new Vector3(0, 0, cube.End.Z - cube.Start.Z).Normalized();
                // rect.Tangent = (0, 0, 1);
                rect.Bitangent = new Vector3(cube.End.X - cube.Start.X, 0, 0).Normalized();
                // rect.Bitangent = (1, 0, 0);
                rect.TextureCoordinateOffset = cube.Start.Xz / 16.0f;
                rect.TextureCoordinateDimensions = rect.Size;
                rect.TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["top"].TextureName]);

                if (cube.Rotation != Vector3.Zero)
                {

                    Matrix3 rotation = Matrix3.CreateRotationZ(Maths.ToRadians(cube.Rotation.Z))
                                     * Matrix3.CreateRotationY(Maths.ToRadians(cube.Rotation.Y))
                                     * Matrix3.CreateRotationX(Maths.ToRadians(cube.Rotation.X));

                    rect.Position -= cube.Origin / 16.0f;
                    rect.Position *= rotation;
                    rect.Tangent *= rotation;
                    rect.Bitangent *= rotation;
                    rect.Position += cube.Origin / 16.0f;

                    model._freeformModelFaces[cube.Properties["top"].CullDirection ?? ResolveDirection(rect)].Add(rect);

                } else
                {

                    if (cube.Properties["top"].ShouldRenderAo)
                    {

                        model._solidModelFaces[cube.Properties["top"].CullDirection ?? Direction.Top].Add(rect);

                    } else
                    {

                        model._freeformModelFaces[cube.Properties["top"].CullDirection ?? Direction.Top].Add(rect);

                    }

                }

            }

            if (cube.Properties["bottom"].IsVisible)
            {

                Rectangle rect = new Rectangle();
                rect.Position = new Vector3(cube.Start.X, cube.Start.Y, cube.End.Z) / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Xz - cube.Start.Xz) / 16.0f;
                rect.Tangent = new Vector3(0, 0, cube.Start.Z - cube.End.Z).Normalized();
                rect.Bitangent = new Vector3(cube.End.X - cube.Start.Z, 0, 0).Normalized();
                rect.TextureCoordinateOffset = cube.Start.Xz / 16.0f;
                rect.TextureCoordinateDimensions = rect.Size;
                rect.TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["bottom"].TextureName]);

                if (cube.Rotation != Vector3.Zero)
                {

                    Matrix3 rotation = Matrix3.CreateRotationZ(Maths.ToRadians(cube.Rotation.Z))
                                     * Matrix3.CreateRotationY(Maths.ToRadians(cube.Rotation.Y))
                                     * Matrix3.CreateRotationX(Maths.ToRadians(cube.Rotation.X));

                    rect.Position -= cube.Origin / 16.0f;
                    rect.Position *= rotation;
                    rect.Tangent *= rotation;
                    rect.Bitangent *= rotation;
                    rect.Position += cube.Origin / 16.0f;

                    model._freeformModelFaces[cube.Properties["bottom"].CullDirection ?? ResolveDirection(rect)].Add(rect);

                } else
                {

                    if (cube.Properties["bottom"].ShouldRenderAo)
                    {

                        model._solidModelFaces[cube.Properties["bottom"].CullDirection ?? Direction.Bottom].Add(rect);

                    } else
                    {

                        model._freeformModelFaces[cube.Properties["bottom"].CullDirection ?? Direction.Bottom].Add(rect);

                    }

                }

            }

            if (cube.Properties["front"].IsVisible)
            {

                Rectangle rect = new Rectangle();
                rect.Position = cube.Start / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Xy - cube.Start.Xy) / 16.0f;
                rect.Tangent = new Vector3(0, cube.End.Y - cube.Start.Y, 0).Normalized();
                rect.Bitangent = new Vector3(cube.End.X - cube.Start.X, 0, 0).Normalized();
                rect.TextureCoordinateOffset = cube.Start.Xy / 16.0f;
                rect.TextureCoordinateDimensions = rect.Size;
                rect.TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["front"].TextureName]);

                if (cube.Rotation != Vector3.Zero)
                {

                    Matrix3 rotation = Matrix3.CreateRotationZ(Maths.ToRadians(cube.Rotation.Z))
                                     * Matrix3.CreateRotationY(Maths.ToRadians(cube.Rotation.Y))
                                     * Matrix3.CreateRotationX(Maths.ToRadians(cube.Rotation.X));

                    rect.Position -= cube.Origin / 16.0f;
                    rect.Position *= rotation;
                    rect.Tangent *= rotation;
                    rect.Bitangent *= rotation;
                    rect.Position += cube.Origin / 16.0f;

                    model._freeformModelFaces[cube.Properties["front"].CullDirection ?? ResolveDirection(rect)].Add(rect);

                } else
                {

                    if (cube.Properties["front"].ShouldRenderAo)
                    {

                        model._solidModelFaces[cube.Properties["front"].CullDirection ?? Direction.Front].Add(rect);

                    } else
                    {

                        model._freeformModelFaces[cube.Properties["front"].CullDirection ?? Direction.Front].Add(rect);

                    }

                }

            }

            if (cube.Properties["left"].IsVisible)
            {

                Rectangle rect = new Rectangle();
                rect.Position = new Vector3(cube.End.X, cube.Start.Y, cube.Start.Z) / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Zy - cube.Start.Zy) / 16.0f;
                rect.Tangent = new Vector3(0, cube.End.Y - cube.Start.Y, 0).Normalized();
                rect.Bitangent = new Vector3(0, 0, cube.End.Z - cube.Start.Z).Normalized();
                rect.TextureCoordinateOffset = cube.Start.Zy / 16.0f;
                rect.TextureCoordinateDimensions = rect.Size;
                rect.TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["left"].TextureName]);

                if (cube.Rotation != Vector3.Zero)
                {

                    Matrix3 rotation = Matrix3.CreateRotationZ(Maths.ToRadians(cube.Rotation.Z))
                                     * Matrix3.CreateRotationY(Maths.ToRadians(cube.Rotation.Y))
                                     * Matrix3.CreateRotationX(Maths.ToRadians(cube.Rotation.X));

                    rect.Position -= cube.Origin / 16.0f;
                    rect.Position *= rotation;
                    rect.Tangent *= rotation;
                    rect.Bitangent *= rotation;
                    rect.Position += cube.Origin / 16.0f;

                    model._freeformModelFaces[cube.Properties["left"].CullDirection ?? ResolveDirection(rect)].Add(rect);

                } else
                {

                    if (cube.Properties["left"].ShouldRenderAo)
                    {

                        model._solidModelFaces[cube.Properties["left"].CullDirection ?? Direction.Left].Add(rect);

                    } else
                    {

                        model._freeformModelFaces[cube.Properties["left"].CullDirection ?? Direction.Left].Add(rect);

                    }

                }

            }

            if (cube.Properties["back"].IsVisible)
            {

                Rectangle rect = new Rectangle();
                rect.Position = new Vector3(cube.End.X, cube.Start.Y, cube.End.Z) / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Xz - cube.Start.Xz) / 16.0f;
                rect.Tangent = new Vector3(0, cube.End.Y - cube.Start.Y, 0).Normalized();
                rect.Bitangent = new Vector3(cube.Start.Z - cube.End.Z, 0, 0).Normalized();
                rect.TextureCoordinateOffset = new Vector2(16.0f - cube.End.X, cube.Start.Y) / 16.0f;
                rect.TextureCoordinateDimensions = rect.Size;
                rect.TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["back"].TextureName]);

                if (cube.Rotation != Vector3.Zero)
                {

                    Matrix3 rotation = Matrix3.CreateRotationZ(Maths.ToRadians(cube.Rotation.Z))
                                     * Matrix3.CreateRotationY(Maths.ToRadians(cube.Rotation.Y))
                                     * Matrix3.CreateRotationX(Maths.ToRadians(cube.Rotation.X));

                    rect.Position -= cube.Origin / 16.0f;
                    rect.Position *= rotation;
                    rect.Tangent *= rotation;
                    rect.Bitangent *= rotation;
                    rect.Position += cube.Origin / 16.0f;

                    model._freeformModelFaces[cube.Properties["back"].CullDirection ?? ResolveDirection(rect)].Add(rect);

                } else
                {

                    if (cube.Properties["back"].ShouldRenderAo)
                    {

                        model._solidModelFaces[cube.Properties["back"].CullDirection ?? Direction.Back].Add(rect);

                    } else
                    {

                        model._freeformModelFaces[cube.Properties["back"].CullDirection ?? Direction.Back].Add(rect);

                    }

                }

            }

            if (cube.Properties["right"].IsVisible)
            {

                Rectangle rect = new Rectangle();
                rect.Position = new Vector3(cube.Start.X, cube.Start.Y, cube.End.Z) / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Zy - cube.Start.Zy) / 16.0f;
                rect.Tangent = new Vector3(0, cube.End.Y - cube.Start.Y, 0).Normalized();
                rect.Bitangent = new Vector3(0, 0, cube.Start.Z - cube.End.Z).Normalized();
                rect.TextureCoordinateOffset = cube.Start.Zy / 16.0f;
                rect.TextureCoordinateDimensions = rect.Size;
                rect.TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["right"].TextureName]);

                if (cube.Rotation != Vector3.Zero)
                {

                    Matrix3 rotation = Matrix3.CreateRotationZ(Maths.ToRadians(cube.Rotation.Z))
                                     * Matrix3.CreateRotationY(Maths.ToRadians(cube.Rotation.Y))
                                     * Matrix3.CreateRotationX(Maths.ToRadians(cube.Rotation.X));

                    rect.Position -= cube.Origin / 16.0f;
                    rect.Position *= rotation;
                    rect.Tangent *= rotation;
                    rect.Bitangent *= rotation;
                    rect.Position += cube.Origin / 16.0f;

                    model._freeformModelFaces[cube.Properties["right"].CullDirection ?? ResolveDirection(rect)].Add(rect);

                } else
                {

                    if (cube.Properties["right"].ShouldRenderAo)
                    {

                        model._solidModelFaces[cube.Properties["right"].CullDirection ?? Direction.Right].Add(rect);

                    } else
                    {

                        model._freeformModelFaces[cube.Properties["right"].CullDirection ?? Direction.Right].Add(rect);

                    }

                }

            }

        }

        /*
        foreach (Cube cube in modelData.Cubes)
        {

            if (model._solidModelFaces.ContainsKey(Direction.Top))
            {
                model._solidModelFaces[Direction.Top].Add(new Rectangle() { 
                    Position = new Vector3(cube.Start.X, cube.End.Y, cube.Start.Z) / 16.0f,
                    Size = Vector2.Abs(cube.End.Xz - cube.Start.Xz) / 16.0f,
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
                    Size = Vector2.Abs(cube.End.Xz - cube.Start.Xz) / 16.0f,
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
                    Position = cube.Start / 16.0f,
                    Size = Vector2.Abs(cube.End.Xy - cube.Start.Xy) / 16.0f,
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
                    Size = Vector2.Abs(cube.End.Xy - cube.Start.Xy) / 16.0f,
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
                    Size = Vector2.Abs(cube.End.Zy - cube.Start.Zy) / 16.0f,
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
                    Size = Vector2.Abs(cube.End.Zy - cube.Start.Zy) / 16.0f,
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
                    Size = Vector2.Abs(cube.End.Xy - cube.Start.Xy) / 16.0f,
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
                    Size = Vector2.Abs(cube.End.Xy - cube.Start.Xy) / 16.0f,
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
                    Size = Vector2.Abs(cube.End.Zy - cube.Start.Zy) / 16.0f,
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
                    Size = Vector2.Abs(cube.End.Zy - cube.Start.Zy) / 16.0f,
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
                    Size = Vector2.Abs(cube.End.Xz - cube.Start.Xz) / 16.0f,
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
                    Size = Vector2.Abs(cube.End.Xz - cube.Start.Xz) / 16.0f,
                    Tangent = (0, 0, -1),
                    Bitangent = (1, 0, 0),
                    TextureCoordinateOffset = cube.Start.Xz / 16.0f,
                    TextureCoordinateDimensions = (cube.End.Xz - cube.Start.Xz) / 16.0f,
                    TextureIndex = (uint) TexturePackManager.GetTextureIndex(modelData.Textures[cube.Properties["bottom"].TextureName])
                }]);
            }
        }
        */
        
        return model;

    }

    private static Direction ResolveDirection(Rectangle rect)
    {

        Vector3 normal = Vector3.Cross(rect.Tangent, rect.Bitangent).Normalized();
        if (normal.X == -0) normal.X = 0;
        if (normal.Y == -0) normal.Y = 0;
        if (normal.Z == -0) normal.Z = 0;

        switch (normal)
        {
            case (0, 1, 0):
                return Direction.Top;
            case (0, -1, 0):
                return Direction.Bottom;
            case (1, 0, 0):
                return Direction.Left;
            case (-1, 0, 0):
                return Direction.Right;
            case (0, 0, 1):
                return Direction.Back;
            case (0, 0, -1):
                return Direction.Front;
            default:
                return Direction.None;
        }

    }

}