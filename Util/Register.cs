using System.Collections.Generic;
using VoxelGame.Util;

namespace VoxelGame;

public class Register
{
    private Dictionary<string, Block> _blocks = new();

    public int BlockCount => _blocks.Count;
    
    public void RegisterBlock(string id, Block block)
    {
        block.Id = id;
        if (!_blocks.TryAdd(id, block))
        {
            Logger.Warning($"A block with the id \"{id}\" already exists. skipping");
        }
        else
        {
            Logger.Info($"Added block {id}");
        }
    }

    public Block GetBlockFromId(string name)
    {
        return _blocks[name];
    }
}