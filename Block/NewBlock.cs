using System;
using System.Collections;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Language;
using Blockgame_OpenTK.Core.Networking;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Tomlet;
using Tomlet.Attributes;
using Tomlet.Models;

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

    public Namespace(string fullName)
    {

        string[] split = fullName.Split('.');
        Prefix = split[0];
        Suffix = split[1];

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
public class NewBlock
{

    [TomlProperty("is_solid")]
    public bool IsSolid { get; set; } = true;

    [TomlNonSerialized]
    public NewBlockModel BlockModel { get; set; }
    [TomlNonSerialized]
    public ushort Id;
    [TomlProperty("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [TomlNonSerialized]
    public Namespace Namespace;

    [TomlProperty("block_model")]
    private string _blockModelPath { get; set; }
    public NewBlock()
    {
        
    }

    public static NewBlock FromToml<T>(string file) where T : NewBlock, new()
    {

        NewBlock properties = TomletMain.To<NewBlock>(File.ReadAllText(file));
        T block = new();

        block.DisplayName = LanguageManager.GetTranslation(properties.DisplayName);
        if (!NetworkingValues.Server?.IsNetworked ?? false) block.BlockModel = NewBlockModel.FromToml(properties._blockModelPath);
        
        return block;

    }

    public virtual void OnBlockSet(World world, Vector3i globalBlockPosition)
    {
        // if (IsSolid) ChunkUtils.SetLightColor(world.PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], ChunkUtils.PositionToBlockLocal(globalBlockPosition), LightColor.Zero);
        world.SetBlock(globalBlockPosition, this);
    }
    public virtual void OnBlockDestroy(World world, Vector3i globalBlockPosition)
    {
        
        world.SetBlock(globalBlockPosition, new NewBlock() {IsSolid = false});
        // world.RemoveLight(globalBlockPosition);
        world.QueueChunk(globalBlockPosition);
        
    }

    public virtual void OnBlockPlace(World world, Vector3i globalBlockPosition)
    {

        world.SetBlock(globalBlockPosition, this);
        // if (IsSolid) world.RemoveLight(globalBlockPosition);
        world.QueueChunk(globalBlockPosition);
        
    }

}