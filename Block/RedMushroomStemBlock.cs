using System.Collections.Generic;
using System.IO;
using Game.Core.Worlds;
using OpenTK.Mathematics;

namespace Game.BlockUtil;

public class RedMushroomStemBlock : Block
{

    private BlockModel _thickStemCenter = new BlockModel().AddCube(new Cube()).SetAllTextures(0, "RedMushroomStem").Generate();

    public override void OnBlockMesh(World world, Vector3i globalBlockPosition, List<Rectangle> solids, List<Rectangle> cutouts)
    {
        
        world.AddModel(_thickStemCenter, globalBlockPosition, solids, cutouts);

    }

}