using Game.Core.Worlds;
using Game.Util;
using OpenTK.Mathematics;

namespace Game.BlockUtil;

public class FixedTickBlock : Block
{

    public override void OnBlockPlace(World world, Vector3i globalBlockPosition, bool shouldUpdateMesh, bool hasPriority)
    {

        base.OnBlockPlace(world, globalBlockPosition, shouldUpdateMesh, hasPriority);
        world.AddBlockTicker(globalBlockPosition, 30);

    }

    public override void OnFixedTick(World world, Vector3i globalBlockPosition, bool shouldUpdateMesh, bool hasPriority)
    {
        
        GameLogger.Log("Hello from FixedTickBlock!");

    }

}