using System.Collections.Generic;

namespace VoxelGame;

public class Register
{
    private Dictionary<string, Block> _blockIndices = new();

    public int BlockCount => _blockIndices.Count;
    
    public void RegisterBlock(string name, Block block)
    {
        block.Id = name;
        _blockIndices.Add(name, block);
    }

    public Block GetBlockFromNamespace(string name)
    {
        return _blockIndices[name];
    }
}