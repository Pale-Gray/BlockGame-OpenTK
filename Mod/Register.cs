using System;
using System.Collections.Generic;
using Game.BlockUtil;
using Game.Core.Generation;

namespace Game.Core.Modding;

public class Register
{

    private Dictionary<string, ushort> _blockLookup = new();
    private List<Block> _blocks = new();
    private Dictionary<string, Biome> _biomeLookup = new();
    private Dictionary<string, Structure> _structureLookup = new();
    public int BlockCount => _blocks.Count;
    private static Block _missingBlock = new Block() { DisplayName = "Game.Block.MissingBlock", BlockModel = new BlockModel().AddCube(new Cube()).SetAllTextures(0, "MissingModel") };
    
    public void RegisterBlock(string @namespace, Block block)
    {
        
        if (@namespace.Contains('.') && @namespace.Split('.').Length < 1) throw new Exception("The namespace is not in a valid format.");

        _blocks.Add(block);
        block.Id = (ushort)_blocks.IndexOf(block);
        block.Namespace = @namespace;
        if (block.BlockModel == null)
        {
            block.BlockModel = new BlockModel().AddCube(new Cube()).Generate();
        }
        _blockLookup.Add(@namespace.ToString(), (ushort) _blocks.IndexOf(block));
        
    }

    public void RegisterStructure(string @namespace, Structure structure)
    {

        _structureLookup.Add(@namespace, structure);

    }

    public void RegisterBiome(string @namespace, Biome biome)
    {

        _biomeLookup.Add(@namespace, biome);

    }
    
    public Structure GetStructureFromNamespace(string @namespace)
    {

        if (_structureLookup.TryGetValue(@namespace, out Structure structure)) return structure;
        return null;

    }

    public Biome GetBiomeFromNamespace(string @namespace)
    {

        if (_biomeLookup.TryGetValue(@namespace, out Biome biome)) return biome;
        return null;

    }

    public Block GetBlockFromId(ushort id)
    {

        if (_blocks.Count > id)
        {

            return _blocks[id];

        }
        return _missingBlock;
        
    }

    public Block GetBlockFromNamespace(string @namespace)
    {

        if (_blockLookup.TryGetValue(@namespace, out ushort id)) return _blocks[id];
        return _missingBlock;

    }
    
}