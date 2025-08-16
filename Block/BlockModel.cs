using System.Collections.Generic;
using OpenTK.Mathematics;

namespace VoxelGame;

public class BlockModel
{
    private List<Cube> _cubes = new();

    private Dictionary<Direction, List<ChunkVertex>> _faces = new()
    {
        { Direction.Top, new() },
        { Direction.Bottom, new() },
        { Direction.Left, new() },
        { Direction.Right, new() },
        { Direction.Front, new() },
        { Direction.Back, new() }
    };
    
    public BlockModel AddCube(Cube cube)
    {
        _cubes.Add(cube);
        return this;
    }

    public BlockModel SetTextureFace(int cubeIndex, Direction face, string textureName)
    {
        _cubes[cubeIndex].FaceResourceLocations[(int)face] = textureName;
        return this;
    }
    
    public BlockModel SetTextureSides(int cubeIndex, string textureName)
    {
        _cubes[cubeIndex].FaceResourceLocations[(int)Direction.Back] = textureName;
        _cubes[cubeIndex].FaceResourceLocations[(int)Direction.Front] = textureName;
        _cubes[cubeIndex].FaceResourceLocations[(int)Direction.Left] = textureName;
        _cubes[cubeIndex].FaceResourceLocations[(int)Direction.Right] = textureName;
        return this;
    }
    
    public BlockModel SetAllTextureFaces(int cubeIndex, string textureName)
    {
        _cubes[cubeIndex].FaceResourceLocations[(int)Direction.Top] = textureName;
        _cubes[cubeIndex].FaceResourceLocations[(int)Direction.Bottom] = textureName;
        _cubes[cubeIndex].FaceResourceLocations[(int)Direction.Back] = textureName;
        _cubes[cubeIndex].FaceResourceLocations[(int)Direction.Front] = textureName;
        _cubes[cubeIndex].FaceResourceLocations[(int)Direction.Left] = textureName;
        _cubes[cubeIndex].FaceResourceLocations[(int)Direction.Right] = textureName;
        return this;
    }

    public BlockModel SetTextureFace(int cubeIndex, Direction face, Vector2i textureCoordinate)
    {
        _cubes[cubeIndex].FaceTextureCoordinates[(int)face] = textureCoordinate;
        return this;
    }

    public BlockModel SetTextureSides(int cubeIndex, Vector2i textureCoordinate)
    {
        _cubes[cubeIndex].FaceTextureCoordinates[(int)Direction.Back] = textureCoordinate;
        _cubes[cubeIndex].FaceTextureCoordinates[(int)Direction.Front] = textureCoordinate;
        _cubes[cubeIndex].FaceTextureCoordinates[(int)Direction.Left] = textureCoordinate;
        _cubes[cubeIndex].FaceTextureCoordinates[(int)Direction.Right] = textureCoordinate;
        return this;
    }

    public BlockModel SetAllTextureFaces(int cubeIndex, Vector2i textureCoordinate)
    {
        _cubes[cubeIndex].FaceTextureCoordinates[(int)Direction.Top] = textureCoordinate;
        _cubes[cubeIndex].FaceTextureCoordinates[(int)Direction.Bottom] = textureCoordinate;
        _cubes[cubeIndex].FaceTextureCoordinates[(int)Direction.Back] = textureCoordinate;
        _cubes[cubeIndex].FaceTextureCoordinates[(int)Direction.Front] = textureCoordinate;
        _cubes[cubeIndex].FaceTextureCoordinates[(int)Direction.Left] = textureCoordinate;
        _cubes[cubeIndex].FaceTextureCoordinates[(int)Direction.Right] = textureCoordinate;
        return this;
    }

    public void AddFace(List<ChunkVertex> data, Direction faceDirection, Vector3i offset)
    {
        data.AddRange(_faces[faceDirection]);
        for (int i = data.Count - _faces[faceDirection].Count; i < data.Count; i++)
        {
            ChunkVertex vertex = data[i];
            vertex.Position += offset;
            data[i] = vertex;
        }
    }
    
    public void Generate()
    {
        foreach (Cube cube in _cubes)
        {
            _faces[Direction.Top].AddRange([
                new ChunkVertex((0, 1, 1), Vector3i.UnitY, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Top])[0]),
                new ChunkVertex((0, 1, 0), Vector3i.UnitY, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Top])[1]),
                new ChunkVertex((1, 1, 0), Vector3i.UnitY, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Top])[2]),
                new ChunkVertex((1, 1, 0), Vector3i.UnitY, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Top])[2]),
                new ChunkVertex((1, 1, 1), Vector3i.UnitY, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Top])[3]),
                new ChunkVertex((0, 1, 1), Vector3i.UnitY, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Top])[0])
            ]);
            
            _faces[Direction.Bottom].AddRange([
                new ChunkVertex((0, 0, 0), -Vector3i.UnitY, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Bottom])[0]),
                new ChunkVertex((0, 0, 1), -Vector3i.UnitY, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Bottom])[1]),
                new ChunkVertex((1, 0, 1), -Vector3i.UnitY, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Bottom])[2]),
                new ChunkVertex((1, 0, 1), -Vector3i.UnitY, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Bottom])[2]),
                new ChunkVertex((1, 0, 0), -Vector3i.UnitY, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Bottom])[3]),
                new ChunkVertex((0, 0, 0), -Vector3i.UnitY, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Bottom])[0])
            ]);
            
            _faces[Direction.Front].AddRange([
                new ChunkVertex((0, 1, 0), -Vector3i.UnitZ, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Front])[0]),
                new ChunkVertex((0, 0, 0), -Vector3i.UnitZ, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Front])[1]),
                new ChunkVertex((1, 0, 0), -Vector3i.UnitZ, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Front])[2]),
                new ChunkVertex((1, 0, 0), -Vector3i.UnitZ, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Front])[2]),
                new ChunkVertex((1, 1, 0), -Vector3i.UnitZ, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Front])[3]),
                new ChunkVertex((0, 1, 0), -Vector3i.UnitZ, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Front])[0])
            ]);
            
            _faces[Direction.Back].AddRange([
                new ChunkVertex((1, 1, 1), Vector3i.UnitZ, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Back])[0]),
                new ChunkVertex((1, 0, 1), Vector3i.UnitZ, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Back])[1]),
                new ChunkVertex((0, 0, 1), Vector3i.UnitZ, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Back])[2]),
                new ChunkVertex((0, 0, 1), Vector3i.UnitZ, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Back])[2]),
                new ChunkVertex((0, 1, 1), Vector3i.UnitZ, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Back])[3]),
                new ChunkVertex((1, 1, 1), Vector3i.UnitZ, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Back])[0])
            ]);
            
            _faces[Direction.Left].AddRange([
                new ChunkVertex((0, 1, 1), -Vector3i.UnitX, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Left])[0]),
                new ChunkVertex((0, 0, 1), -Vector3i.UnitX, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Left])[1]),
                new ChunkVertex((0, 0, 0), -Vector3i.UnitX, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Left])[2]),
                new ChunkVertex((0, 0, 0), -Vector3i.UnitX, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Left])[2]),
                new ChunkVertex((0, 1, 0), -Vector3i.UnitX, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Left])[3]),
                new ChunkVertex((0, 1, 1), -Vector3i.UnitX, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Left])[0])
            ]);
            
            _faces[Direction.Right].AddRange([
                new ChunkVertex((1, 1, 0), Vector3i.UnitX, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Right])[0]),
                new ChunkVertex((1, 0, 0), Vector3i.UnitX, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Right])[1]),
                new ChunkVertex((1, 0, 1), Vector3i.UnitX, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Right])[2]),
                new ChunkVertex((1, 0, 1), Vector3i.UnitX, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Right])[2]),
                new ChunkVertex((1, 1, 1), Vector3i.UnitX, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Right])[3]),
                new ChunkVertex((1, 1, 0), Vector3i.UnitX, Config.Atlas.GetTextureCoordinates(cube.FaceResourceLocations[(int)Direction.Right])[0])
            ]);
        }
    }
}