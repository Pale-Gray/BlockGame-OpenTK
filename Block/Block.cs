using System.Collections.Generic;
using Game.Core.Worlds;
using Game.Util;
using OpenTK.Mathematics;

namespace Game.BlockUtil;
public class Block
{

    public bool IsSolid { get; set; } = true;
    public BlockModel BlockModel { get; set; }
    public ushort Id { get; set; } = 0;
    public string DisplayName { get; set; } = string.Empty;
    public string Namespace;
    private string _blockModelPath { get; set; }
    public Block()
    {
        
    }

    public Block(string translationKey, BlockModel model)
    {

        DisplayName = translationKey;
        BlockModel = model;

    }

    public virtual void OnBlockDestroy(World world, Vector3i globalBlockPosition, bool shouldUpdateMesh, bool hasPriority)
    {
        
        world.SetBlock(globalBlockPosition, GlobalValues.Register.GetBlockFromId(0), shouldUpdateMesh, hasPriority);
        world.RemoveBlockProperty(globalBlockPosition);
        world.RemoveBlockTicker(globalBlockPosition);
        
    }

    public virtual void OnBlockPlace(World world, Vector3i globalBlockPosition, bool shouldUpdateMesh, bool hasPriority)
    {

        world.SetBlock(globalBlockPosition, this, shouldUpdateMesh, hasPriority);
        
    }

    public virtual void OnBlockMesh(World world, Vector3i globalBlockPosition, List<Rectangle> solids, List<Rectangle> cutouts)
    {

        world.AddModel(BlockModel, globalBlockPosition, solids, cutouts);

    }

    public virtual void OnRandomTick(World world, Vector3i globalBlockPosition, bool shouldUpdateMesh, bool hasPriority)
    {



    }

    public virtual void OnFixedTick(World world, Vector3i globalBlockPosition, bool shouldUpdateMesh, bool hasPriority)
    {



    }

}