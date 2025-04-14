using Game.Core.Chunks;
using Game.Core.Worlds;
using OpenTK.Mathematics;

namespace Game.BlockUtil;

public class RedLightBlock : Block 
{

    public override void OnBlockPlace(World world, Vector3i globalBlockPosition, bool e = true)
    {
        base.OnBlockPlace(world, globalBlockPosition, e);
        world.AddLight(globalBlockPosition, 15, 0, 0);
    }

}