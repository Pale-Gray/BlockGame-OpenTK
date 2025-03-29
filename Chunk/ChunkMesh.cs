using System;
using System.Collections.Generic;
using Game.BlockUtil;
using Game.Core.PlayerUtil;
using Game.Core.TexturePack;
using Game.Core.Worlds;
using Game.PlayerUtil;
using Game.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Game.Core.Chunks;

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

public class ChunkMesh
{

    // public int[] PackedChunkMeshIndices;
    // public PackedChunkVertex[] PackedChunkVertices;
    public List<PackedChunkVertex> ChunkVertices;
    public List<int> ChunkIndices = new();
    public int ChunkIndicesCount;
    public List<Rectangle> Solids = new();
    public int[] PackedChunkBindlessTextureIndices;
    public int Vbo, Vao, Ibo, SolidsHandle;
    public Vector3i ChunkPosition = Vector3i.Zero;
    public bool IsRenderable = false;
    public int IndexCount = 0;

    public ChunkMesh(Vector3i chunkPosition)
    {
        
        ChunkPosition = chunkPosition;
        
    }

    public void Draw(Player player)
    {

        if (ChunkIndices != null && ChunkIndices.Count > 0 && IsRenderable)
        {

            GL.Disable(EnableCap.CullFace);
            GlobalValues.PackedChunkShader.Use();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2dArray, TexturePackManager.ArrayTextureHandle);
            // GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Point);
            // GL.PointSize(10.0f);
            
            GL.Uniform3f(GL.GetUniformLocation(GlobalValues.PackedChunkShader.Handle, "chunkPosition"), ChunkPosition.X, ChunkPosition.Y, ChunkPosition.Z);
            Matrix4 viewMatrix = player.Camera.ViewMatrix;
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.PackedChunkShader.Handle, "view"), 1, true, ref viewMatrix);
            Matrix4 projectionMatrix = player.Camera.ProjectionMatrix;
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.PackedChunkShader.Handle, "projection"), 1, true, ref projectionMatrix);
            Matrix4 conversionMatrix = player.Camera.ConversionMatrix;
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.PackedChunkShader.Handle, "conversion"), 1, true, ref conversionMatrix);

            GL.BindVertexArray(Vao);
            GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, 0, SolidsHandle);
            GL.DrawElements(PrimitiveType.Triangles, ChunkIndices.Count, DrawElementsType.UnsignedInt, 0);
            GL.Enable(EnableCap.CullFace);
            // GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            
        }
        
    }

}