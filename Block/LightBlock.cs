using Game.Core.Worlds;
using OpenTK.Mathematics;

namespace Game.BlockUtil;

public class LightBlock : Block
{

    public override void OnBlockPlace(World world, Vector3i globalBlockPosition, bool shouldUpdateMesh, bool hasPriority)
    {
        base.OnBlockPlace(world, globalBlockPosition, shouldUpdateMesh, hasPriority);
        world.AddLight(globalBlockPosition, 15, 15, 15);
    }

}