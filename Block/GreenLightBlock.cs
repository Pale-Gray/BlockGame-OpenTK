using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.BlockUtil;

public class GreenLightBlock : NewBlock
{

    public override void OnBlockPlace(PackedChunkWorld world, Vector3i globalBlockPosition)
    {
        base.OnBlockPlace(world, globalBlockPosition);
        world.AddLight(globalBlockPosition, new BlockLight(new LightColor(0, 15, 0)));
    }

}