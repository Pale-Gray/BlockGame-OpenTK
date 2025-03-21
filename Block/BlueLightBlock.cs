using Game.Core.Chunks;
using Game.Core.Worlds;
using OpenTK.Mathematics;

namespace Game.BlockUtil;

public class BlueLightBlock : Block 
{

    public override void OnBlockPlace(World world, Vector3i globalBlockPosition)
    {
        base.OnBlockPlace(world, globalBlockPosition);
        world.AddLight(globalBlockPosition, new BlockLight(new LightColor(0, 0, 15)));
    }

}