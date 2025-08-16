using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using VoxelGame.Networking;

namespace VoxelGame;

public class World
{
    public ConcurrentDictionary<Vector2i, Chunk> ChunkColumns = new();

    public void AddColumn(Vector2i position, Chunk column)
    {
        column.Position = position;
        for (int i = 0; i < Config.ColumnSize; i++)
        {
            column.ChunkSections[i].Position = (position.X, i, position.Y);
        }
        ChunkColumns.TryAdd(position, column);
    }

    public void Draw(Camera camera)
    {
        Config.ChunkShader.Bind();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2d, Config.Atlas.Id);
        GL.UniformMatrix4f(Config.ChunkShader.GetUniformLocation("uProjection"), 1, true, ref camera.Projection);
        GL.UniformMatrix4f(Config.ChunkShader.GetUniformLocation("uView"), 1, true, ref camera.View);
        GL.Uniform1f(Config.ChunkShader.GetUniformLocation("uTexture"), 0);
        
        foreach (Chunk column in ChunkColumns.Values)
        {
            for (int i = 0; i < column.ChunkMeshes.Length; i++)
            {
                GL.Uniform3f(Config.ChunkShader.GetUniformLocation("uChunkPosition"), column.ChunkSections[i].Position.X, column.ChunkSections[i].Position.Y, column.ChunkSections[i].Position.Z);
                if (column.ChunkMeshes[i].Length > 0) column.ChunkMeshes[i].Draw(camera);
            }
        }
    }

    public ushort GetBlockId(Vector3i globalBlockPosition)
    {
        Vector3i chunkPosition = ChunkMath.GlobalToChunk(globalBlockPosition);
        if (!ChunkColumns.ContainsKey(chunkPosition.Xz) || globalBlockPosition.Y < 0 || globalBlockPosition.Y >= Config.ChunkSize * Config.ColumnSize) return 0;
        
        return ChunkColumns[chunkPosition.Xz].ChunkSections[chunkPosition.Y].GetBlockId(ChunkMath.GlobalToLocal(globalBlockPosition));
    }

    public void SetBlockId(Vector3i globalBlockPosition, ushort value)
    {
        Vector3i chunkPosition = ChunkMath.GlobalToChunk(globalBlockPosition);
        if (ChunkColumns.ContainsKey(chunkPosition.Xz) && chunkPosition.Y >= 0 &&
            chunkPosition.Y < Config.ColumnSize)
        {
            Vector3i pos = ChunkMath.GlobalToLocal(globalBlockPosition);
            ChunkColumns[chunkPosition.Xz].ChunkSections[chunkPosition.Y].SetBlockId(value, pos.X, pos.Y, pos.Z);
        }
    }

    public void EnqueueChunk(Vector2i chunkPosition)
    {
        
    }
}