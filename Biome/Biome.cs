using System.ComponentModel;
using Game.Core.Worlds;
using OpenTK.Mathematics;

namespace Game.Core.Generation;

public class Biome
{
    public virtual void OnTerrainPass(World world, Vector3i globalBlockPosition)
    {

        

    }

    public virtual void OnFeaturePass(World world, Vector3i globalBlockPosition)
    {



    }

}