using Game.BlockUtil;
using Game.Core.Worlds;
using Game.Util;
using OpenTK.Mathematics;

namespace Game.Core.Generation;

public class Biome
{

    public virtual Block TopSoil => GlobalValues.Register.GetBlockFromNamespace("Game.GrassBlock");
    public virtual Block Dirt => GlobalValues.Register.GetBlockFromNamespace("Game.DirtBlock");
    public virtual Block RockLayerOne => GlobalValues.Register.GetBlockFromNamespace("Game.StoneBlock");

    public virtual void OnTerrainPass(World world, Vector3i globalBlockPosition)
    {

        

    }

    public virtual void OnFeaturePass(World world, Vector3i globalBlockPosition)
    {



    }

}