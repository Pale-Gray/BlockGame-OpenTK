using System;
using System.Collections.Generic;
using System.Linq;

namespace VoxelGame;

public class Register
{
    private Dictionary<string, ushort> _blockIndices = new();
    private List<Block> _blocks = new();

    public int BlockCount => _blocks.Count;
    
    public void RegisterBlock(string name, Block block)
    {
        _blockIndices.Add(name, (ushort) _blocks.Count);
        block.Id = (ushort) _blocks.Count;
        _blocks.Add(block);
        if (Config.Client != null) _blocks[_blocks.Count - 1].BlockModel.Generate();
    }

    public Block GetBlockFromNamespace(string name)
    {
        return _blocks[_blockIndices[name]];
    }

    public Block GetBlockFromId(ushort id)
    {
        return _blocks[id];
    }
}