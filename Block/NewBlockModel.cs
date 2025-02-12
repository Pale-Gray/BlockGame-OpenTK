using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using Tomlet;
using Tomlet.Attributes;
using Tomlet.Models;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace Blockgame_OpenTK.BlockUtil;

public enum Direction : byte
{
    
    None = 0b000000,
    Top = 0b000001,
    Bottom = 0b000010,
    Left = 0b000100,
    Right = 0b001000,
    Back = 0b010000,
    Front = 0b100000
    
}

public struct FaceProperties
{
    
    [TomlProperty("texture_name")] public string TextureName { get; set; }
    [TomlProperty("should_render_ao")] public bool? ShouldRenderAo { get; set; }
    [TomlProperty("is_visible")] public bool? IsVisible { get; set; }

    public FaceProperties() {}

}

public struct Cube
{
    [TomlProperty("start")] public Vector3 Start { get; set; }
    [TomlProperty("end")] public Vector3 End { get; set; }

    [TomlProperty("rotation")] public Vector3 Rotation { get; set; }
    [TomlProperty("origin")] public Vector3 Origin { get; set; }
    
    [TomlProperty("properties")] public Dictionary<string, FaceProperties> Properties { get; set; }

    public string TopTextureName;
    public string BottomTextureName;
    public string LeftTextureName;
    public string RightTextureName;
    public string BackTextureName;
    public string FrontTextureName;

    public Cube() { }
    
    private byte _visibleFlags = 0b111111;

    public Cube(Vector3 start, Vector3 end)
    {
        Start = start;
        End = end;
    }

    public void SetVisible(Direction direction, bool isVisible)
    {

        if (isVisible)
        {
            _visibleFlags = (byte) (_visibleFlags | (byte)direction);
        }
        else
        {
            _visibleFlags = (byte) (_visibleFlags & ~(byte)direction);
        }

    }

    public bool IsVisible(Direction direction)
    {
        
        return (_visibleFlags & (byte)direction) != 0;

    }

}

public class PrimitiveModelData
{

    [TomlProperty("inherit")] public string InheritPath { get; set; }
    [TomlProperty("textures")] public Dictionary<string, string> Textures { get; set; }
    [TomlProperty("cubes")] public List<Cube> Cubes { get; set; }

}

public class NewBlockModel
{
    
    private Dictionary<Direction, List<PackedChunkVertex>> _computedModelPackedVertices = new();
    // private Dictionary<Direction, List<CustomVertex>> _computedModelGeneralVertices = new();
    public bool IsFullBlock = true;

    public List<PackedChunkVertex> QueryPackedFace(Direction direction, Vector3i offset, ConcurrentDictionary<Vector3i, PackedChunk> neighborChunks)
    {
        List<PackedChunkVertex> result = [];
        if (_computedModelPackedVertices.TryGetValue(direction, out List<PackedChunkVertex> packedVertices))
        {
            for (int i = 0; i < packedVertices.Count; i++)
            {
                PackedChunkVertex vertex = packedVertices[i];
                vertex.Position += offset;
                vertex.LightColor = Vector3.One;
                result.Add(vertex);
            }
        }
        return result;

    }

    public static NewBlockModel FromCubes(List<Cube> cubes)
    {
        
        NewBlockModel blockModel = new NewBlockModel();
        
        return blockModel;

    }

    public static NewBlockModel FromToml(string tomlPath)
    {

        TomlSerializerOptions op = new TomlSerializerOptions();
        op.OverrideConstructorValues = false;
        PrimitiveModelData modelData = TomletMain.To<PrimitiveModelData>(File.ReadAllText(Path.Combine(GlobalValues.BlockModelPath, tomlPath)), options: op);
        if (modelData.InheritPath != null)
        {
            string inheritPath = Path.Combine(GlobalValues.BlockModelPath, Path.Combine(modelData.InheritPath.Split('/')));
            PrimitiveModelData inheritData = TomletMain.To<PrimitiveModelData>(File.ReadAllText(inheritPath), options: op);
            if (modelData.Cubes == null) modelData.Cubes = [];
            modelData.Cubes.AddRange(inheritData.Cubes);
        }
        
        return Parse(modelData);
    }

    private static NewBlockModel Parse(PrimitiveModelData modelData)
    {

        NewBlockModel model = new NewBlockModel();

        foreach (Cube cube in modelData.Cubes)
        {

            // can go in packed dictionary
            if (cube.Start == (0, 0, 0) && cube.End == (32, 32, 32) && 
                cube.Rotation == (0, 0, 0) && cube.Origin == (0, 0, 0))
            {

                Vector3i normalizedEnd = (Vector3i) (cube.End / 32.0f);
                Vector3i normalizedStart = (Vector3i) (cube.Start / 32.0f);
                foreach (KeyValuePair<string, FaceProperties> faceProperties in cube.Properties)
                {

                    if (faceProperties.Value.IsVisible ?? true)
                    {

                        switch (faceProperties.Key)
                        {
                            case "top":
                                model._computedModelPackedVertices.Add(Direction.Top, [
                                    new PackedChunkVertex(normalizedEnd, Core.Chunks.Direction.Up, 0, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedEnd - (0, 0, 1),Core.Chunks.Direction.Up, 1, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (0, 1, 0), Core.Chunks.Direction.Up, 2, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (0, 1, 1), Core.Chunks.Direction.Up, 3, modelData.Textures[faceProperties.Value.TextureName]),
                                ]);
                                break;
                            case "bottom":
                                model._computedModelPackedVertices.Add(Direction.Bottom, [
                                    new PackedChunkVertex(normalizedStart + (1, 0, 0), Core.Chunks.Direction.Down, 0, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedEnd - (0, 1, 0), Core.Chunks.Direction.Down, 1, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (0, 0, 1), Core.Chunks.Direction.Down, 2, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart, Core.Chunks.Direction.Down, 3, modelData.Textures[faceProperties.Value.TextureName])
                                ]);
                                break;
                            case "right":
                                model._computedModelPackedVertices.Add(Direction.Right, [
                                    new PackedChunkVertex(normalizedStart + (0, 1, 0), Core.Chunks.Direction.Right, 0, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart, Core.Chunks.Direction.Right, 1, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (0, 0, 1), Core.Chunks.Direction.Right, 2, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (0, 1, 1), Core.Chunks.Direction.Right, 3, modelData.Textures[faceProperties.Value.TextureName])
                                ]);
                                break;
                            case "left":
                                model._computedModelPackedVertices.Add(Direction.Left, [
                                    new PackedChunkVertex(normalizedEnd, Core.Chunks.Direction.Left, 0, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedEnd - (0, 1, 0), Core.Chunks.Direction.Left, 1, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (1, 0, 0), Core.Chunks.Direction.Left, 2, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedEnd - (0, 0, 1), Core.Chunks.Direction.Left, 3, modelData.Textures[faceProperties.Value.TextureName])
                                ]);
                                break;
                            case "back":
                                model._computedModelPackedVertices.Add(Direction.Back, [
                                    new PackedChunkVertex(normalizedStart + (0, 1, 1), Core.Chunks.Direction.Back, 0, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (0, 0, 1), Core.Chunks.Direction.Back, 1, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedEnd - (0, 1, 0), Core.Chunks.Direction.Back, 2, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedEnd, Core.Chunks.Direction.Back, 3, modelData.Textures[faceProperties.Value.TextureName])
                                ]);
                                break;
                            case "front":
                                model._computedModelPackedVertices.Add(Direction.Front, [
                                    new PackedChunkVertex(normalizedStart + (1, 1, 0), Core.Chunks.Direction.Front, 0, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (1, 0, 0), Core.Chunks.Direction.Front, 1, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart, Core.Chunks.Direction.Front, 2, modelData.Textures[faceProperties.Value.TextureName]),
                                    new PackedChunkVertex(normalizedStart + (0, 1, 0), Core.Chunks.Direction.Front, 3, modelData.Textures[faceProperties.Value.TextureName])
                                ]);
                                break;
                            
                        }
                        
                    }
                    
                }
                
            }
            else
            {
                
                
                
            }
            
        }
        
        return model;

    }

}