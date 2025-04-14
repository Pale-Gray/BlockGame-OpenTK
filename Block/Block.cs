using System;
using System.Collections;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using Game.Core.Chunks;
using Game.Core.Language;
using Game.Core.Networking;
using Game.Core.Worlds;
using Game.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Tomlet;
using Tomlet.Attributes;
using Tomlet.Models;

namespace Game.BlockUtil;
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

    public Namespace(string fullName)
    {

        string[] split = fullName.Split('.');
        Prefix = split[0];
        Suffix = split[1];

    }   

    public static implicit operator Namespace(string name)
    {

        string[] namespaceString = name.Split('.');

        return new Namespace(namespaceString[0], namespaceString[1]);

    }

    public override int GetHashCode()
    {
        
        return ToString().GetHashCode();

    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        
        Namespace? value = obj as Namespace?;
        if (value == null) return false;
        if (value?.ToString() == ToString()) return true;
        return false;

    }

}
public class Block
{

    [TomlProperty("is_solid")]
    public bool IsSolid { get; set; } = true;

    [TomlNonSerialized]
    public BlockModel BlockModel { get; set; }
    [TomlNonSerialized]
    public ushort Id;
    [TomlProperty("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [TomlNonSerialized]
    public Namespace Namespace;

    [TomlProperty("block_model")]
    private string _blockModelPath { get; set; }
    public Block()
    {
        
    }

    public static Block FromToml<T>(string file) where T : Block, new()
    {

        Block properties = TomletMain.To<Block>(File.ReadAllText(file));
        T block = new();

        block.DisplayName = LanguageManager.GetTranslation(properties.DisplayName);
        block.IsSolid = properties.IsSolid;
        if (NetworkingValues.Server == null) block.BlockModel = BlockModel.FromToml(properties._blockModelPath);
        
        return block;

    }

    public virtual void OnBlockDestroy(World world, Vector3i globalBlockPosition, bool isPlayerPlaced = true)
    {
        
        world.SetBlock(globalBlockPosition, GlobalValues.Register.GetBlockFromId(0), isPlayerPlaced);
        
    }

    public virtual void OnBlockPlace(World world, Vector3i globalBlockPosition, bool isPlayerPlaced = true)
    {

        world.SetBlock(globalBlockPosition, this, isPlayerPlaced);
        
    }

    public virtual void OnBlockMesh(World world, Vector3i globalBlockPosition)
    {

        

    }

    public virtual void OnRandomTick(World world, Vector3i globalBlockPosition, bool isPlayerPlaced = true)
    {



    }

}