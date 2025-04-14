using Game.Core.Chunks;
using Game.Core.Worlds;
using OpenTK.Mathematics;

namespace Game.BlockUtil;

public class GreenLightBlock : Block
{

    public override void OnBlockPlace(World world, Vector3i globalBlockPosition, bool e = true)
    {
        base.OnBlockPlace(world, globalBlockPosition);
        world.AddLight(globalBlockPosition, 0, 15, 0);
    }

}