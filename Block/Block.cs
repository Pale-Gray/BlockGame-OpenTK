using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Networking;

namespace VoxelGame;

public class Block
{
    public BlockModel Model = new BlockModel();
    public BoundingBox BoundingBox = new BoundingBox();
    public ushort Id = 0;
    public bool IsSolid = true;
    
    public Block SetBlockModel(BlockModel model)
    {
        Model = model;
        return this;
    }

    public virtual void OnBlockPlace(World world, Vector3i blockPosition)
    {
        world.SetBlockId(blockPosition, Id);
        world.EnqueueChunksFromBlockPosition(blockPosition);
    }

    public virtual void OnBlockDestroy(World world, Vector3i blockPosition)
    {
        world.SetBlockId(blockPosition, 0);
        world.EnqueueChunksFromBlockPosition(blockPosition);
    }

    public virtual void OnBlockMesh(World world, Vector3i blockPosition)
    {
        world.AddModel(blockPosition, Model);
    }
}