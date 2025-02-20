using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.BlockUtil;

public class BlueLightBlock : NewBlock 
{

    public override void OnBlockPlace(PackedChunkWorld world, Vector3i globalBlockPosition)
    {
        world.AddLight(globalBlockPosition, new BlockLight(new LightColor(0, 0, 15)));
        base.OnBlockPlace(world, globalBlockPosition);
    }

    public override void OnBlockDestroy(PackedChunkWorld world, Vector3i globalBlockPosition)
    {
        world.RemoveLight(globalBlockPosition, new LightColor(0, 0, 15));
        base.OnBlockDestroy(world, globalBlockPosition);
    }

}