using System;
using System.Collections.Generic;
using Blockgame_OpenTK.Core.TexturePack;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.PlayerUtil;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Chunks;

public enum Direction : uint
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
    Front = 4,
    Back = 5
}

public struct PackedChunkVertex
{

    public uint PackedVertexInfo = 0;
    public uint PackedExtraInfo = 0;
    public Vector3 LightColor = Vector3.One;
    public float SunlightColor = 1.0f;
    public Vector3i Position
    {
        get
        {
            uint xValue = (PackedVertexInfo >> 17) & 0b111111;
            uint yValue = (PackedVertexInfo >> 11) & 0b111111;
            uint zValue = (PackedVertexInfo >> 5) & 0b111111;
            return ((int)xValue, (int)yValue, (int)zValue);
        }
        set
        {
            uint val = ((uint)value.X << 12);
            val |= (uint)value.Y << 6;
            val |= (uint)value.Z;
            PackedVertexInfo = (PackedVertexInfo & 0b11111) | val << 5;
        }
    }
    public Direction Normal
    {
        get => (Direction)((PackedVertexInfo >> 2) & 0b111);
        set => PackedVertexInfo = (uint) (PackedVertexInfo & ~0b11100) | (((uint)value) << 2);
    }

    public uint TextureCoordinateIndex
    {
        get => (PackedVertexInfo & 3);
        set => PackedVertexInfo = (uint) (PackedVertexInfo & ~3) | (value & 3);
    }

    public int BindlessTextureIndex;

    public PackedChunkVertex(Vector3i position, Direction normal)
    {
        Position = position;
        Normal = normal;
        PackedExtraInfo = (ushort)TexturePackManager.GetTextureIndex("MissingModel");
    }
    
    public PackedChunkVertex(Vector3i position, Direction normal, uint textureCoordinateIndex, string textureName)
    {
        Position = position;
        Normal = normal;
        TextureCoordinateIndex = textureCoordinateIndex;
        PackedExtraInfo = (ushort)TexturePackManager.GetTextureIndex(textureName);
    }
    
    public PackedChunkVertex(Vector3i position, Direction normal, uint textureCoordinateIndex, string textureName, Vector3 lightColor)
    {
        Position = position;
        Normal = normal;
        TextureCoordinateIndex = textureCoordinateIndex;
        PackedExtraInfo = (ushort)TexturePackManager.GetTextureIndex(textureName);
        LightColor = lightColor;
    }
    
}

public struct ChunkVertex
{

    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TextureCoordinate;

    public ChunkVertex(Vector3 position, Vector3 normal, Vector2 texCoord)
    {

        Position = position;
        Normal = normal;
        TextureCoordinate = texCoord;

    }

}

public class PackedChunkMesh
{

    // public int[] PackedChunkMeshIndices;
    // public PackedChunkVertex[] PackedChunkVertices;
    public List<PackedChunkVertex> ChunkVertices;
    public List<int> ChunkIndices;
    public int[] PackedChunkBindlessTextureIndices;
    public int Vbo, Vao, Ibo, Ssbo, Ssbo2;
    public Vector3i ChunkPosition = Vector3i.Zero;
    public bool IsRenderable = false;

    public PackedChunkMesh(Vector3i chunkPosition)
    {
        
        ChunkPosition = chunkPosition;
        
    }

    public void Draw(Player player)
    {
        
        if (ChunkIndices != null && ChunkIndices.Count > 0 && IsRenderable)
        {
            
            GlobalValues.PackedChunkShader.Use();
            GL.BindTexture(TextureTarget.Texture2dArray, TexturePackManager.ArrayTextureName);
            
            GL.Uniform3f(GL.GetUniformLocation(GlobalValues.PackedChunkShader.id, "chunkPosition"), ChunkPosition.X, ChunkPosition.Y, ChunkPosition.Z);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.PackedChunkShader.id, "view"), 1, true, ref player.Camera.ViewMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.PackedChunkShader.id, "projection"), 1, true, ref player.Camera.ProjectionMatrix);

            GL.BindVertexArray(Vao);
            GL.DrawElements(PrimitiveType.Triangles, ChunkIndices.Count, DrawElementsType.UnsignedInt, 0);
            
        } else
        { 

        }
        
    }

}