using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Resources;
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

public enum FaceDirection
{

    Top,
    Bottom,
    Left,
    Right,
    Back,
    Front

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
    public Vector2 TextureSize;
    [FieldOffset(120)]
    public Vector2 TextureOffset;
    [FieldOffset(128)]
    public Vector2 Size;
    [FieldOffset(136)]
    public uint TextureIndex;
}

public struct FaceProperties
{
    
    public string TextureName { get; set; } = "MissingTexture";
    public bool ShouldRenderAo { get; set; } = true;
    public bool IsVisible { get; set; } = true;
    public Direction? CullDirection { get; set; } = null;

    public FaceProperties() {}

}

public struct Cube
{
    public Vector3 Start { get; set; } = Vector3.Zero;
    public Vector3 End { get; set; } = (16, 16, 16);

    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Origin { get; set; } = (8, 8, 8);
    
    public Dictionary<string, FaceProperties> Properties { get; set; }

    public Dictionary<FaceDirection, FaceProperties> CubeProperties;

    public Cube() 
    { 

        CubeProperties = new()
        {
            {FaceDirection.Top, new FaceProperties()},
            {FaceDirection.Bottom, new FaceProperties()},
            {FaceDirection.Front, new FaceProperties()},
            {FaceDirection.Left, new FaceProperties()},
            {FaceDirection.Back, new FaceProperties()},
            {FaceDirection.Right, new FaceProperties()}
        };  

    }
    
    private byte _visibleFlags = 0b111111;

    public Cube(Vector3 start, Vector3 end)
    {
        Start = start;
        End = end;
        CubeProperties = new()
        {
            {FaceDirection.Top, new FaceProperties()},
            {FaceDirection.Bottom, new FaceProperties()},
            {FaceDirection.Front, new FaceProperties()},
            {FaceDirection.Left, new FaceProperties()},
            {FaceDirection.Back, new FaceProperties()},
            {FaceDirection.Right, new FaceProperties()}
        };  
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

    public Cube Copy()
    {

        Cube cube = new Cube();

        cube.Start = Start;
        cube.End = End;
        cube.Rotation = Rotation;
        cube.Origin = Origin;

        foreach (KeyValuePair<FaceDirection, FaceProperties> property in CubeProperties)
        {

            cube.CubeProperties[property.Key] = property.Value;

        }

        return cube;

    }

}

public class PrimitiveModelData
{

    public string InheritPath { get; set; }
    public Dictionary<string, string> Textures { get; set; }
    public List<Cube> Cubes { get; set; }

}

public class BlockModel
{
    
    private Dictionary<Direction, List<Rectangle>> _solidModelFaces = new();
    private Dictionary<Direction, List<Rectangle>> _cutoutModelFaces = new();
    private Dictionary<Direction, List<Rectangle>> _freeformModelFaces = new();
    private List<Cube> _primitiveModelData = new();
    public bool IsFullBlock = true;

    public BlockModel AddCube(Cube cube)
    {

        _primitiveModelData.Add(cube);
        return this;

    }

    public BlockModel SetTexture(int cubeIndex, FaceDirection faceDirection, string textureName)
    {

        FaceProperties properties = _primitiveModelData[cubeIndex].CubeProperties[faceDirection];
        properties.TextureName = textureName;
        _primitiveModelData[cubeIndex].CubeProperties[faceDirection] = properties;
        return this;

    }

    public BlockModel SetAllTextures(int cubeIndex, string textureName)
    {

        foreach (FaceDirection direction in Enum.GetValues(typeof(FaceDirection)))
        {

            FaceProperties properties = _primitiveModelData[cubeIndex].CubeProperties[direction];
            properties.TextureName = textureName;
            _primitiveModelData[cubeIndex].CubeProperties[direction] = properties;

        }

        return this;

    }

    public BlockModel SetVisible(int cubeIndex, FaceDirection faceDirection, bool isVisible)
    {

        FaceProperties properties = _primitiveModelData[cubeIndex].CubeProperties[faceDirection];
        properties.IsVisible = isVisible;
        _primitiveModelData[cubeIndex].CubeProperties[faceDirection] = properties;
        return this;

    }

    public BlockModel SetRotation(int cubeIndex, Vector3 angles)
    {

        Cube cube = _primitiveModelData[cubeIndex];
        cube.Rotation = angles;
        _primitiveModelData[cubeIndex] = cube;
        return this;

    }

    public BlockModel Copy()
    {

        BlockModel model = new BlockModel();

        foreach (Cube cube in _primitiveModelData)
        {
            model._primitiveModelData.Add(cube.Copy());
        }

        return model;

    }

    // NOTE:
    // tangent is the "left" direction
    // bitangent is the "up" direction

    public BlockModel Generate()
    {

        foreach (Cube cube in _primitiveModelData)
        {

            Matrix3 rotation = Matrix3.CreateRotationZ(Maths.ToRadians(cube.Rotation.Z)) *
                               Matrix3.CreateRotationY(Maths.ToRadians(cube.Rotation.Y)) *
                               Matrix3.CreateRotationX(Maths.ToRadians(cube.Rotation.X));

            if (cube.CubeProperties[FaceDirection.Top].IsVisible && cube.Start.Y == cube.End.Y)
            {

                Rectangle rect = new Rectangle();
                rect.Position = new Vector3(cube.Start.X, cube.End.Y, cube.Start.Z) / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Xz - cube.Start.Xz) / 16.0f;
                rect.Tangent = new Vector3(cube.End.X - cube.Start.X, 0, 0).Normalized();
                rect.Bitangent = new Vector3(0, 0, cube.End.Z - cube.Start.Z).Normalized();
                rect.TextureOffset = (1.0f - (cube.End.X / 16.0f), rect.Position.Z);
                rect.TextureSize = rect.Size;
                rect.TextureIndex = TexturePackManager.GetTextureIndex(cube.CubeProperties[FaceDirection.Top].TextureName);

                rect.Position -= cube.Origin / 16.0f;
                rect.Position *= rotation;
                rect.Tangent *= rotation;
                rect.Bitangent *= rotation;
                rect.Position += cube.Origin / 16.0f;

                _cutoutModelFaces[cube.CubeProperties[FaceDirection.Top].CullDirection ?? CalculateCullingDirection(rect)].Add(rect);
                continue;

            }

            if (cube.CubeProperties[FaceDirection.Front].IsVisible && cube.Start.Z == cube.End.Z)
            {

                Rectangle rect = new Rectangle();
                rect.Position = cube.Start / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Xy - cube.Start.Xy) / 16.0f;
                rect.Tangent = new Vector3(cube.End.X - cube.Start.X, 0, 0).Normalized();
                rect.Bitangent = new Vector3(0, cube.End.Y - cube.Start.Y, 0).Normalized();
                rect.TextureOffset = (1.0f - (cube.End.X / 16.0f), rect.Position.Y);
                rect.TextureSize = rect.Size;
                rect.TextureIndex = TexturePackManager.GetTextureIndex(cube.CubeProperties[FaceDirection.Front].TextureName);

                rect.Position -= cube.Origin / 16.0f;
                rect.Position *= rotation;
                rect.Tangent *= rotation;
                rect.Bitangent *= rotation;
                rect.Position += cube.Origin / 16.0f;

                _cutoutModelFaces[cube.CubeProperties[FaceDirection.Front].CullDirection ?? CalculateCullingDirection(rect)].Add(rect);
                continue;

            }

            if (cube.CubeProperties[FaceDirection.Left].IsVisible && cube.Start.X == cube.End.X)
            {

                Rectangle rect = new Rectangle();
                rect.Position = new Vector3(cube.End.X, cube.Start.Y, cube.Start.Z) / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Zy - cube.Start.Zy) / 16.0f;
                rect.Tangent = new Vector3(0, 0, cube.End.Z - cube.Start.Z).Normalized();
                rect.Bitangent = new Vector3(0, cube.End.Y - cube.Start.Y, 0).Normalized();
                rect.TextureOffset = (1.0f - (cube.End.Z / 16.0f), rect.Position.Y);
                rect.TextureSize = rect.Size;
                rect.TextureIndex = TexturePackManager.GetTextureIndex(cube.CubeProperties[FaceDirection.Left].TextureName);

                rect.Position -= cube.Origin / 16.0f;
                rect.Position *= rotation;
                rect.Tangent *= rotation;
                rect.Bitangent *= rotation;
                rect.Position += cube.Origin / 16.0f;

                _cutoutModelFaces[cube.CubeProperties[FaceDirection.Left].CullDirection ?? CalculateCullingDirection(rect)].Add(rect);
                continue;

            }

            if (cube.CubeProperties[FaceDirection.Top].IsVisible)
            {

                Rectangle rect = new Rectangle();
                rect.Position = new Vector3(cube.Start.X, cube.End.Y, cube.Start.Z) / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Xz - cube.Start.Xz) / 16.0f;
                rect.Tangent = new Vector3(cube.End.X - cube.Start.X, 0, 0).Normalized();
                rect.Bitangent = new Vector3(0, 0, cube.End.Z - cube.Start.Z).Normalized();
                rect.TextureOffset = (1.0f - (cube.End.X / 16.0f), rect.Position.Z);
                rect.TextureSize = rect.Size;
                rect.TextureIndex = TexturePackManager.GetTextureIndex(cube.CubeProperties[FaceDirection.Top].TextureName);

                if (cube.Rotation != Vector3.Zero)
                {

                    rect.Position -= cube.Origin / 16.0f;
                    rect.Position *= rotation;
                    rect.Tangent *= rotation;
                    rect.Bitangent *= rotation;
                    rect.Position += cube.Origin / 16.0f;

                    _freeformModelFaces[cube.CubeProperties[FaceDirection.Top].CullDirection ?? CalculateCullingDirection(rect)].Add(rect);

                } else
                {

                    _solidModelFaces[cube.CubeProperties[FaceDirection.Top].CullDirection ?? CalculateCullingDirection(rect)].Add(rect);

                }

            }

            if (cube.CubeProperties[FaceDirection.Bottom].IsVisible)
            {

                Rectangle rect = new Rectangle();
                rect.Position = new Vector3(cube.Start.X, cube.Start.Y, cube.End.Z) / 16.0f;
                rect.Size = Vector2.Abs(cube.Start.Xz - cube.End.Xz) / 16.0f;
                rect.Tangent = new Vector3(cube.End.X - cube.Start.X, 0, 0).Normalized();
                rect.Bitangent = new Vector3(0, 0, cube.Start.Z - cube.End.Z).Normalized();
                rect.TextureOffset = (1.0f - (cube.End.X / 16.0f), 1.0f - (cube.End.Z / 16.0f));
                rect.TextureSize = rect.Size;
                rect.TextureIndex = TexturePackManager.GetTextureIndex(cube.CubeProperties[FaceDirection.Bottom].TextureName);

                if (cube.Rotation != Vector3.Zero)
                {

                    rect.Position -= cube.Origin / 16.0f;
                    rect.Position *= rotation;
                    rect.Tangent *= rotation;
                    rect.Bitangent *= rotation;
                    rect.Position += cube.Origin / 16.0f;

                    _freeformModelFaces[cube.CubeProperties[FaceDirection.Bottom].CullDirection ?? CalculateCullingDirection(rect)].Add(rect);

                } else
                {

                    _solidModelFaces[cube.CubeProperties[FaceDirection.Bottom].CullDirection ?? CalculateCullingDirection(rect)].Add(rect);

                }

            }

            if (cube.CubeProperties[FaceDirection.Front].IsVisible)
            {

                Rectangle rect = new Rectangle();
                rect.Position = cube.Start / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Xy - cube.Start.Xy) / 16.0f;
                rect.Tangent = new Vector3(cube.End.X - cube.Start.X, 0, 0).Normalized();
                rect.Bitangent = new Vector3(0, cube.End.Y - cube.Start.Y, 0).Normalized();
                rect.TextureOffset = (1.0f - (cube.End.X / 16.0f), rect.Position.Y);
                rect.TextureSize = rect.Size;
                rect.TextureIndex = TexturePackManager.GetTextureIndex(cube.CubeProperties[FaceDirection.Front].TextureName);

                if (cube.Rotation != Vector3.Zero)
                {

                    rect.Position -= cube.Origin / 16.0f;
                    rect.Position *= rotation;
                    rect.Tangent *= rotation;
                    rect.Bitangent *= rotation;
                    rect.Position += cube.Origin / 16.0f;

                    _freeformModelFaces[cube.CubeProperties[FaceDirection.Front].CullDirection ?? CalculateCullingDirection(rect)].Add(rect);

                } else
                {

                    _solidModelFaces[cube.CubeProperties[FaceDirection.Front].CullDirection ?? CalculateCullingDirection(rect)].Add(rect);

                }

            }

            if (cube.CubeProperties[FaceDirection.Left].IsVisible)
            {

                Rectangle rect = new Rectangle();
                rect.Position = new Vector3(cube.End.X, cube.Start.Y, cube.Start.Z) / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Zy - cube.Start.Zy) / 16.0f;
                rect.Tangent = new Vector3(0, 0, cube.End.Z - cube.Start.Z).Normalized();
                rect.Bitangent = new Vector3(0, cube.End.Y - cube.Start.Y, 0).Normalized();
                rect.TextureOffset = (1.0f - (cube.End.Z / 16.0f), rect.Position.Y);
                rect.TextureSize = rect.Size;
                rect.TextureIndex = TexturePackManager.GetTextureIndex(cube.CubeProperties[FaceDirection.Left].TextureName);

                if (cube.Rotation != Vector3.Zero)
                {

                    rect.Position -= cube.Origin / 16.0f;
                    rect.Position *= rotation;
                    rect.Tangent *= rotation;
                    rect.Bitangent *= rotation;
                    rect.Position += cube.Origin / 16.0f;

                    _freeformModelFaces[cube.CubeProperties[FaceDirection.Left].CullDirection ?? CalculateCullingDirection(rect)].Add(rect);

                } else
                {

                    _solidModelFaces[cube.CubeProperties[FaceDirection.Left].CullDirection ?? CalculateCullingDirection(rect)].Add(rect);

                }

            }

            if (cube.CubeProperties[FaceDirection.Back].IsVisible)
            {

                Rectangle rect = new Rectangle();
                rect.Position = new Vector3(cube.End.X, cube.Start.Y, cube.End.Z) / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Xy - cube.Start.Xy) / 16.0f;
                rect.Tangent = new Vector3(cube.Start.X - cube.End.X, 0, 0).Normalized();
                rect.Bitangent = new Vector3(0, cube.End.Y - cube.Start.Y, 0).Normalized();
                rect.TextureOffset = cube.Start.Xy / 16.0f;
                rect.TextureSize = rect.Size;
                rect.TextureIndex = TexturePackManager.GetTextureIndex(cube.CubeProperties[FaceDirection.Back].TextureName);

                if (cube.Rotation != Vector3.Zero)
                {

                    rect.Position -= cube.Origin / 16.0f;
                    rect.Position *= rotation;
                    rect.Tangent *= rotation;
                    rect.Bitangent *= rotation;
                    rect.Position += cube.Origin / 16.0f;

                    _freeformModelFaces[cube.CubeProperties[FaceDirection.Back].CullDirection ?? CalculateCullingDirection(rect)].Add(rect);

                } else
                {

                    _solidModelFaces[cube.CubeProperties[FaceDirection.Back].CullDirection ?? CalculateCullingDirection(rect)].Add(rect);

                }

            }   

            if (cube.CubeProperties[FaceDirection.Right].IsVisible)
            {

                Rectangle rect = new Rectangle();
                rect.Position = new Vector3(cube.Start.X, cube.Start.Y, cube.End.Z) / 16.0f;
                rect.Size = Vector2.Abs(cube.End.Zy - cube.Start.Zy) / 16.0f;
                rect.Tangent = new Vector3(0, 0, cube.Start.Z - cube.End.Z).Normalized();
                rect.Bitangent = new Vector3(0, cube.End.Y - cube.Start.Y, 0).Normalized();
                rect.TextureOffset = (cube.Start.Z / 16.0f, cube.Start.Y / 16.0f);
                rect.TextureSize = rect.Size;
                rect.TextureIndex = TexturePackManager.GetTextureIndex(cube.CubeProperties[FaceDirection.Right].TextureName);

                if (cube.Rotation != Vector3.Zero)
                {

                    rect.Position -= cube.Origin / 16.0f;
                    rect.Position *= rotation;
                    rect.Tangent *= rotation;
                    rect.Bitangent *= rotation;
                    rect.Position += cube.Origin / 16.0f;

                    _freeformModelFaces[cube.CubeProperties[FaceDirection.Back].CullDirection ?? CalculateCullingDirection(rect)].Add(rect);

                } else
                {

                    _solidModelFaces[cube.CubeProperties[FaceDirection.Back].CullDirection ?? CalculateCullingDirection(rect)].Add(rect);

                }

            }

        }

        return this;

    }

    private Direction CalculateCullingDirection(Rectangle rect)
    {

        Vector3 normal = Vector3.Cross(rect.Bitangent, rect.Tangent);
        
        Console.WriteLine($"tangent: {rect.Tangent}, bitangent: {rect.Bitangent}, normal: {normal}");

        if (normal == Vector3.UnitY) return Direction.Top;
        if (normal == -Vector3.UnitY) return Direction.Bottom;
        if (normal == Vector3.UnitX) return Direction.Left;
        if (normal == -Vector3.UnitX) return Direction.Right;
        if (normal == Vector3.UnitZ) return Direction.Back;
        if (normal == -Vector3.UnitZ) return Direction.Front;

        Console.WriteLine("err");

        return Direction.None;

    }

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

}