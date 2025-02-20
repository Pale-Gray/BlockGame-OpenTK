using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Net;
using System.Threading;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Language;
using Blockgame_OpenTK.Core.Worlds;
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
        block.BlockModel = NewBlockModel.FromToml(properties._blockModelPath);

        return block;

    }

    public virtual void OnBlockSet(PackedChunkWorld world, Vector3i globalBlockPosition)
    {
        if (IsSolid) ChunkUtils.SetLightColor(world.PackedWorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)], ChunkUtils.PositionToBlockLocal(globalBlockPosition), LightColor.Zero);
        world.SetBlock(globalBlockPosition, this);
    }
    public virtual void OnBlockDestroy(PackedChunkWorld world, Vector3i globalBlockPosition)
    {
        
        world.SetBlock(globalBlockPosition, new NewBlock() {IsSolid = false});
        world.RemoveLight(globalBlockPosition, LightColor.Zero);
        world.QueueChunk(globalBlockPosition);
        
    }

    public virtual void OnBlockPlace(PackedChunkWorld world, Vector3i globalBlockPosition)
    {

        world.SetBlock(globalBlockPosition, this);
        if (IsSolid) world.RemoveLight(globalBlockPosition, LightColor.Zero);
        world.QueueChunk(globalBlockPosition);
        
    }

}