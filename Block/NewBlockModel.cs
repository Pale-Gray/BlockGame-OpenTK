using System;
using System.Collections.Generic;
using System.Numerics;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace Blockgame_OpenTK.BlockUtil;

public enum Direction : byte
{
    
    Top = 0b000001,
    Bottom = 0b000010,
    Left = 0b000100,
    Right = 0b001000,
    Back = 0b010000,
    Front = 0b100000
    
}

public struct Cube
{

    public Vector3 Start;
    public Vector3 End;

    public Vector3 Rotation = Vector3.Zero;
    public Vector3 Origin = Vector3.Zero;

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

        return (_visibleFlags & (byte)direction) == 1;

    }

}

public class NewBlockModel
{

    private List<Cube> _cubes = [];
    public bool IsSolid = true;
    public bool IsFullBlock
    {
        get
        {
            if (_cubes.Count == 0)
            {
                if (_cubes[0].Start == Vector3.Zero && _cubes[0].End == Vector3.Zero && _cubes[0].Rotation == Vector3.Zero) return true;
            }
            return true;
        }
    }

    public PackedChunkVertex[] QueryPackedFace(Direction direction, Vector3i offset)
    {

        PackedChunkVertex[] vertices = new PackedChunkVertex[4];

        switch (direction)
        {
            case Direction.Top:
                vertices[0] = new PackedChunkVertex((1, 1, 1) + offset, Core.Chunks.Direction.Up, _cubes[0].TopTextureName);
                vertices[1] = new PackedChunkVertex((1, 1, 0) + offset, Core.Chunks.Direction.Up, _cubes[0].TopTextureName);
                vertices[2] = new PackedChunkVertex((0, 1, 0) + offset, Core.Chunks.Direction.Up, _cubes[0].TopTextureName);
                vertices[3] = new PackedChunkVertex((0, 1, 1) + offset, Core.Chunks.Direction.Up, _cubes[0].TopTextureName);
                break;
            case Direction.Bottom:
                vertices[0] = new PackedChunkVertex((1, 0, 0)+ offset, Core.Chunks.Direction.Down, _cubes[0].BottomTextureName);
                vertices[1] = new PackedChunkVertex((1, 0, 1)+ offset, Core.Chunks.Direction.Down, _cubes[0].BottomTextureName);
                vertices[2] = new PackedChunkVertex((0, 0, 1)+ offset, Core.Chunks.Direction.Down, _cubes[0].BottomTextureName);
                vertices[3] = new PackedChunkVertex((0, 0, 0)+ offset, Core.Chunks.Direction.Down, _cubes[0].BottomTextureName);
                break;
            case Direction.Left:
                vertices[0] = new PackedChunkVertex((1, 1, 1)+ offset, Core.Chunks.Direction.Left, _cubes[0].LeftTextureName);
                vertices[1] = new PackedChunkVertex((1, 0, 1)+ offset, Core.Chunks.Direction.Left, _cubes[0].LeftTextureName);
                vertices[2] = new PackedChunkVertex((1, 0, 0)+ offset, Core.Chunks.Direction.Left, _cubes[0].LeftTextureName);
                vertices[3] = new PackedChunkVertex((1, 1, 0)+ offset, Core.Chunks.Direction.Left, _cubes[0].LeftTextureName);
                break;
            case Direction.Right:
                vertices[0] = new PackedChunkVertex((0, 1, 0)+ offset, Core.Chunks.Direction.Right, _cubes[0].RightTextureName);
                vertices[1] = new PackedChunkVertex((0, 0, 0)+ offset, Core.Chunks.Direction.Right, _cubes[0].RightTextureName);
                vertices[2] = new PackedChunkVertex((0, 0, 1)+ offset, Core.Chunks.Direction.Right, _cubes[0].RightTextureName);
                vertices[3] = new PackedChunkVertex((0, 1, 1)+ offset, Core.Chunks.Direction.Right, _cubes[0].RightTextureName);
                break;
            case Direction.Back:
                vertices[0] = new PackedChunkVertex((0, 1, 1)+ offset, Core.Chunks.Direction.Back, _cubes[0].BackTextureName);
                vertices[1] = new PackedChunkVertex((0, 0, 1)+ offset, Core.Chunks.Direction.Back, _cubes[0].BackTextureName);
                vertices[2] = new PackedChunkVertex((1, 0, 1)+ offset, Core.Chunks.Direction.Back, _cubes[0].BackTextureName);
                vertices[3] = new PackedChunkVertex((1, 1, 1)+ offset, Core.Chunks.Direction.Back, _cubes[0].BackTextureName);
                break;
            case Direction.Front:
                vertices[0] = new PackedChunkVertex((1, 1, 0)+ offset, Core.Chunks.Direction.Front, _cubes[0].FrontTextureName);
                vertices[1] = new PackedChunkVertex((1, 0, 0)+ offset, Core.Chunks.Direction.Front, _cubes[0].FrontTextureName);
                vertices[2] = new PackedChunkVertex((0, 0, 0)+ offset, Core.Chunks.Direction.Front, _cubes[0].FrontTextureName);
                vertices[3] = new PackedChunkVertex((0, 1, 0)+ offset, Core.Chunks.Direction.Front, _cubes[0].FrontTextureName);
                break;
        }
        
        return vertices;

    }

    public static NewBlockModel FromCubes(List<Cube> cubes)
    {
        
        NewBlockModel blockModel = new NewBlockModel();

        blockModel._cubes = cubes;
        
        return blockModel;

    }

}