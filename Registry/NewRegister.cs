using System.Collections.Generic;
using Blockgame_OpenTK.BlockUtil;

namespace Blockgame_OpenTK.Registry;

public class NewRegister
{

    private Dictionary<Namespace, ushort> _namespaceLookup = new();
    private List<NewBlock> _blocks = new();
    
    private static NewBlock _missingBlock = new NewBlock();
    
    public void RegisterBlock(Namespace nameSpace, NewBlock block)
    {
        
        _blocks.Add(block);
        block.Id = (ushort)_blocks.IndexOf(block);
        _namespaceLookup.Add(nameSpace, (ushort) _blocks.IndexOf(block));
        
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

        if (_namespaceLookup.TryGetValue(name, out ushort id))
        {

            return _blocks[id];

        } 
        return _missingBlock;
        
    }
    
}