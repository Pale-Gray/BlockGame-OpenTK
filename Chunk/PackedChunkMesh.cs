using System;
using System.Collections.Generic;
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
    public Vector3i Position
    {
        get
        {
            uint xValue = (PackedVertexInfo >> 15) & 0b111111;
            uint yValue = (PackedVertexInfo >> 9) & 0b111111;
            uint zValue = (PackedVertexInfo >> 3) & 0b111111;
            return ((int)xValue, (int)yValue, (int)zValue);
        }
        set
        {
            uint val = ((uint)value.X << 12);
            val |= (uint)value.Y << 6;
            val |= (uint)value.Z;
            PackedVertexInfo = (PackedVertexInfo & 0b111) | val << 3;
        }
    }
    public Direction Normal
    {
        get => (Direction)(PackedVertexInfo & 0b111);
        set => PackedVertexInfo = (uint) (PackedVertexInfo & ~0b111) | (uint)value;
    }

    public PackedChunkVertex(Vector3i position, Direction normal)
    {
        Position = position;
        Normal = normal;
        PackedExtraInfo = (ushort)GlobalValues.ArrayTexture.GetTextureIndex("MissingModel");
    }
    
    public PackedChunkVertex(Vector3i position, Direction normal, string textureName)
    {
        Position = position;
        Normal = normal;
        PackedExtraInfo = (ushort)GlobalValues.ArrayTexture.GetTextureIndex(textureName);
    }
    
}

public class PackedChunkMesh
{

    public int[] PackedChunkMeshIndices;
    public PackedChunkVertex[] PackedChunkVertices;
    public int Vbo, Vao, Ibo;
    public Vector3i ChunkPosition = Vector3i.Zero;
    public bool IsRenderable = false;

    public PackedChunkMesh(Vector3i chunkPosition)
    {
        
        ChunkPosition = chunkPosition;
        
    }

    public void Draw(Player player)
    {

        // BlockGame.rmodel.SetScale(16, 16, 16);
        // if (IsRenderable) BlockGame.rmodel.Draw(((Vector3)ChunkPosition + (0.5f, 0.5f, 0.5f)) * 32, Vector3.Zero, player.Camera, 1.0f);
        
        if (PackedChunkMeshIndices != null && PackedChunkMeshIndices.Length > 0 && IsRenderable)
        {
            
            GlobalValues.PackedChunkShader.Use();
            GL.BindTexture(TextureTarget.Texture2dArray, GlobalValues.ArrayTexture.TextureID);
            
            GL.Uniform3f(GL.GetUniformLocation(GlobalValues.PackedChunkShader.id, "chunkPosition"), ChunkPosition.X, ChunkPosition.Y, ChunkPosition.Z);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.PackedChunkShader.id, "view"), 1, true, ref player.Camera.ViewMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.PackedChunkShader.id, "projection"), 1, true, ref player.Camera.ProjectionMatrix);
            
            GL.BindVertexArray(Vao);
            GL.DrawElements(PrimitiveType.Triangles, PackedChunkMeshIndices.Length, DrawElementsType.UnsignedInt, 0);
            
        }
        
    }

}