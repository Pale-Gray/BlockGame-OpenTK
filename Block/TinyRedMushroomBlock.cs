using System.Collections.Generic;
using Game.Core.Generation;
using Game.Core.Modding;
using Game.Core.Worlds;
using OpenTK.Mathematics;

namespace Game.BlockUtil;

public class TinyRedMushroomBlock : Block
{

    private BlockModel _variant1 = new BlockModel().AddCube(new Cube((3, 0, 3), (5, 4, 5))).SetAllTextures(0, "TinyRedMushroomSide").SetVisible(0, FaceDirection.Top, false).SetVisible(0, FaceDirection.Bottom, false)
                                                                                                         .AddCube(new Cube((1, 4, 1), (7, 6, 7))).SetAllTextures(1, "TinyRedMushroom")
                                                                                                         .AddCube(new Cube((10, 0, 10), (12, 6, 12))).SetAllTextures(2, "TinyRedMushroomSide")
                                                                                                         .AddCube(new Cube((8, 6, 8), (14, 8, 14))).SetAllTextures(3, "TinyRedMushroom").Generate();
    private BlockModel _variant2 = new BlockModel().AddCube(new Cube((7, 0, 7), (9, 6, 9))).SetAllTextures(0, "TinyRedMushroomSide").SetVisible(0, FaceDirection.Bottom, false).SetVisible(0, FaceDirection.Top, false)
                                                   .AddCube(new Cube((5, 6, 5), (11, 8, 11))).SetAllTextures(1, "TinyRedMushroom")
                                                   .Generate();

    private BlockModel _variant3 = new BlockModel().AddCube(new Cube((7, 0, 7), (9, 4, 9))).SetAllTextures(0, "TinyRedMushroomSide").SetVisible(0, FaceDirection.Bottom, false).SetVisible(0, FaceDirection.Top, false)
                                                   .AddCube(new Cube((5, 4, 5), (11, 6, 11))).SetAllTextures(1, "TinyRedMushroom")
                                                   .Generate();                                          

    public override void OnBlockMesh(World world, Vector3i globalBlockPosition, List<Rectangle> solids, List<Rectangle> cutouts)
    {
        
        switch (Noise.IntRandom3(0, globalBlockPosition, 0, 3))
        {

            case 0:
                world.AddModel(_variant1, globalBlockPosition, solids, cutouts);
                break;
            case 1:
                world.AddModel(_variant2, globalBlockPosition, solids, cutouts);
                break;
            case 2:
                world.AddModel(_variant3, globalBlockPosition, solids, cutouts);
                break;

        }

    }

}