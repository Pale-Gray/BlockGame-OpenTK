using System.Collections.Generic;
using Game.Core.Worlds;
using OpenTK.Mathematics;

namespace Game.BlockUtil;

public class RedMushroomCapBlock : Block
{

    private BlockModel _cap = new BlockModel().AddCube(new Cube()).SetAllTextures(0, "RedMushroomCap").Generate();
    private BlockModel _gills = new BlockModel().AddCube(new Cube()).SetAllTextures(0, "RedMushroomGills").Generate();

    public override void OnBlockMesh(World world, Vector3i globalBlockPosition, List<Rectangle> solids, List<Rectangle> cutouts)
    {
        
        if (world.GetSolidBlock(globalBlockPosition + Vector3i.UnitY))
        {

            world.AddModel(_gills, globalBlockPosition, solids, cutouts);

        } else
        {

            world.AddModel(_cap, globalBlockPosition, solids, cutouts);

        }

    }

}