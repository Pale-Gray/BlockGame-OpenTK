using System;
using System.Collections.Generic;
using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK.Registry;

public class NewRegister
{

    private Dictionary<string, ushort> _namespaceLookup = new();
    private List<NewBlock> _blocks = new();
    
    public int BlockCount => _blocks.Count;
    
    private static NewBlock _missingBlock = new NewBlock();
    
    public void RegisterBlock(Namespace nameSpace, NewBlock block)
    {
        
        GameLogger.Log($"NewRegistering block {nameSpace}");
        _blocks.Add(block);
        block.Id = (ushort)_blocks.IndexOf(block);
        block.Namespace = nameSpace;
        if (block.BlockModel != null)
        {
            GameLogger.Log($"{nameSpace} has a model");
            // block.BlockModel.GenerateVertices();
        }
        _namespaceLookup.Add(nameSpace.ToString(), (ushort) _blocks.IndexOf(block));
        
    }

    public NewBlock GetBlockFromId(ushort id)
    {

        if (_blocks.Count > id)
        {

            return _blocks[id];

        }
        return _missingBlock;
        
    }

    public NewBlock GetBlockFromNamespace(Namespace name)
    {

        if (_namespaceLookup.TryGetValue(name.ToString(), out ushort id))
        {

            return _blocks[id];

        } 
        return _missingBlock;
        
    }

    public NewBlock GetBlockFromNamespace(string name)
    {

        if (_namespaceLookup.TryGetValue(name, out ushort id)) return _blocks[id];
        return _missingBlock;

    }
    
}