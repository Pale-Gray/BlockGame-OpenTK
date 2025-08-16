using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Networking;

namespace VoxelGame;

public class Block
{
    public BlockModel BlockModel = new BlockModel();
    public BoundingBox BoundingBox = new BoundingBox();
    public ushort Id = 0;
    
    public Block SetBlockModel(BlockModel model)
    {
        BlockModel = model;
        return this;
    }

    public virtual void OnBlockPlace(World world, Vector3i blockPosition)
    {
        world.SetBlockId(blockPosition, Id);
    }

    public virtual void OnBlockDestroy(World world, Vector3i blockPosition)
    {
        world.SetBlockId(blockPosition, 0);
    }
}

public enum Direction
{
    Top,
    Bottom,
    Left,
    Right,
    Front,
    Back
}

public struct Cube
{
    public Vector3 Position = Vector3.Zero;
    public Vector3 Size = Vector3.One;
    public Vector2i[] FaceTextureCoordinates = new Vector2i[6];
    public string[] FaceResourceLocations = new string[6];

    public Cube()
    {
        
    }
    
    public Cube(Vector3 position, Vector3 size)
    {
        Position = position;
        Size = size;
    }
}