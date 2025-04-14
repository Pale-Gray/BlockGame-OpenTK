using Game.Core.Worlds;
using OpenTK.Mathematics;

namespace Game.BlockUtil;

public class BlueLightBlock : Block 
{

    public override void OnBlockPlace(World world, Vector3i globalBlockPosition, bool e)
    {
        base.OnBlockPlace(world, globalBlockPosition);
        world.AddLight(globalBlockPosition, 0, 0, 15);
    }

}