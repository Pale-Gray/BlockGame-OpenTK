using System;
using System.Collections.Generic;
using Game.BlockUtil;
using Game.Util;

namespace Game.Registry;

public class Register
{

    private Dictionary<string, ushort> _namespaceLookup = new();
    private List<Block> _blocks = new();
    
    public int BlockCount => _blocks.Count;
    
    private static Block _missingBlock = new Block();
    
    public void RegisterBlock(Namespace nameSpace, Block block)
    {
        
        _blocks.Add(block);
        block.Id = (ushort)_blocks.IndexOf(block);
        block.Namespace = nameSpace;
        if (block.BlockModel != null)
        {
            // GameLogger.Log($"{nameSpace} has a model");
            // block.BlockModel.GenerateVertices();
        }
        _namespaceLookup.Add(nameSpace.ToString(), (ushort) _blocks.IndexOf(block));
        
    }

    public Block GetBlockFromId(ushort id)
    {

        if (_blocks.Count > id)
        {

            return _blocks[id];

        }
        return _missingBlock;
        
    }

    public Block GetBlockFromNamespace(Namespace name)
    {

        if (_namespaceLookup.TryGetValue(name.ToString(), out ushort id))
        {

            return _blocks[id];

        } 
        return _missingBlock;
        
    }

    public Block GetBlockFromNamespace(string name)
    {

        if (_namespaceLookup.TryGetValue(name, out ushort id)) return _blocks[id];
        Console.WriteLine("err");
        return _missingBlock;

    }
    
}