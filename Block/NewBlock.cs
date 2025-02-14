using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.BlockUtil;

public struct Namespace
{

    public string Prefix;
    public string Suffix;

    public override string ToString()
    {
        return $"{Prefix}.{Suffix}";
    }

    public Namespace(string prefix, string suffix)
    {
        
        Prefix = prefix;
        Suffix = suffix;
        
    }

}
public class NewBlock
{

    public bool IsSolid = true;
    public NewBlockModel BlockModel;
    public ushort Id;
    public string DisplayName;
    public Namespace Namespace;
    
    public NewBlock()
    {
        
    }

    public virtual void OnBlockSet(PackedChunkWorld world, Vector3i globalBlockPosition)
    {
        world.SetBlock(globalBlockPosition, this);
    }
    public virtual void OnBlockDestroy(PackedChunkWorld world, Vector3i globalBlockPosition)
    {
        
        world.SetBlock(globalBlockPosition, new NewBlock() {IsSolid = false});
        if (IsSolid) world.RemoveLight(globalBlockPosition);
        world.QueueChunk(globalBlockPosition);
        
    }

    public virtual void OnBlockPlace(PackedChunkWorld world, Vector3i globalBlockPosition)
    {
        
        world.SetBlock(globalBlockPosition, this);
        world.QueueChunk(globalBlockPosition);
        
    }

}