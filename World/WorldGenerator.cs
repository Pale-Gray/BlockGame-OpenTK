using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace VoxelGame;

public class WorldGenerator
{
    private World _world;
    private List<Thread> _generatorThreads = new();
    private AutoResetEvent _generatorResetEvent = new AutoResetEvent(true);
    private bool _shouldMesh;
    private bool _shouldRun = true;

    public ConcurrentQueue<Vector2i> GenerationQueue = new();
    public ConcurrentQueue<Vector2i> SendChunkDataQueue = new();
    public ConcurrentQueue<Vector2i> UploadQueue = new();
    
    public WorldGenerator(World world, bool shouldMesh = true)
    {
        _world = world;
        _shouldMesh = shouldMesh;
    }

    public WorldGenerator Start()
    {
        for (int i = 0; i < 2; i++)
        {
            _generatorThreads.Add(new Thread(HandleGenerationQueue));
            _generatorThreads[i].Start();
        }
        
        return this;
    }

    public WorldGenerator Stop()
    {
        _shouldRun = false;
        foreach (Thread thr in _generatorThreads)
        {
            while (thr.IsAlive) _generatorResetEvent.Set();
        }

        return this;
    }

    public void Poll()
    {
        if (GenerationQueue.Count > 0) _generatorResetEvent.Set();
        
        while (UploadQueue.TryDequeue(out Vector2i position))
        {
            UploadMesh(_world.ChunkColumns[position]);
        }
    }

    private void HandleGenerationQueue()
    {
        while (_shouldRun)
        {
            _generatorResetEvent.WaitOne();
            if (GenerationQueue.TryDequeue(out Vector2i position))
            {
                Chunk column = _world.ChunkColumns[position];
                switch (column.Status)
                {
                    case ChunkStatus.Empty:
                        GenerateColumn(column);
                        break;
                    case ChunkStatus.Mesh:
                        if (_shouldMesh)
                        {
                            if (ChunkMath.ChebyshevDistance(position, Vector2i.Zero) < 8)
                            {
                                if (AreNeighborsTheSameStatus(position, ChunkStatus.Mesh))
                                {
                                    GenerateMesh(_world, column);
                                }
                                else
                                {
                                    GenerationQueue.Enqueue(position);
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
    
    public bool AreNeighborsTheSameStatus(Vector2i position, ChunkStatus status)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if ((x, z) != Vector2i.Zero)
                {
                    if (!_world.ChunkColumns.ContainsKey(position + (x, z)) ||
                        _world.ChunkColumns[position + (x, z)].Status < status)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    
    public void GenerateColumn(Chunk column)
    {
        for (int x = 0; x < Config.ChunkSize; x++)
        {
            for (int z = 0; z < Config.ChunkSize; z++)
            {
                Vector2i globalPosition = (x + (column.Position.X * Config.ChunkSize), z + (column.Position.Y * Config.ChunkSize));
                int height = (int) (32.0 * Noise.ValueNoise2(0, (Vector2)globalPosition / 64.0f));
                for (int y = Config.ChunkSize * Config.ColumnSize - 1; y >= 0; y--)
                {
                    if (y <= 128 + height)
                    {
                        Chunk.SetBlock(column, (x,y,z), Config.Register.GetBlockFromNamespace("pumpkin"));
                    }
                }
            }
        }

        column.Status = ChunkStatus.Mesh;
        GenerationQueue.Enqueue(column.Position);
    }
    
    public void GenerateMesh(World world, Chunk column)
    {
        for (int i = 0; i < Config.ColumnSize; i++)
        {
            ChunkSectionMesh mesh = column.ChunkMeshes[i];
            if (!mesh.NeedsUpdates) continue;
            
            for (int x = 0; x < Config.ChunkSize; x++)
            {
                for (int y = 0; y < Config.ChunkSize; y++)
                {
                    for (int z = 0; z < Config.ChunkSize; z++)
                    {
                        ushort id = column.ChunkSections[i].GetBlockId(x, y, z);
                        if (id != 0)
                        {
                            if (y == Config.ChunkSize - 1 && i == Config.ColumnSize - 1)
                            {
                                Config.Register.GetBlockFromId(id).BlockModel.AddFace(mesh.Data, Direction.Top, (x,y,z));
                            }
                            else if (column.ChunkSections[i + ChunkMath.GlobalToChunk((x,y + 1,z)).Y].GetBlockId(ChunkMath.GlobalToLocal((x,y +1,z))) == 0)
                            {
                                Config.Register.GetBlockFromId(id).BlockModel.AddFace(mesh.Data, Direction.Top, (x,y,z));
                            }
                            
                            if (y == 0 && i == 0)
                            {
                                Config.Register.GetBlockFromId(id).BlockModel.AddFace(mesh.Data, Direction.Bottom, (x,y,z));
                            } else if (column.ChunkSections[i + ChunkMath.GlobalToChunk((x, y - 1, z)).Y ].GetBlockId(ChunkMath.GlobalToLocal((x, y - 1, z))) == 0)
                            {
                                Config.Register.GetBlockFromId(id).BlockModel.AddFace(mesh.Data, Direction.Bottom, (x,y,z));
                            }
                            
                            if (world.ChunkColumns[column.Position + ChunkMath.GlobalToChunk((x, y, z - 1)).Xz].ChunkSections[i]
                                    .GetBlockId(ChunkMath.GlobalToLocal((x, y, z - 1))) == 0)
                            {
                                Config.Register.GetBlockFromId(id).BlockModel.AddFace(mesh.Data, Direction.Front, (x,y,z));
                            }
                            
                            if (world.ChunkColumns[column.Position + ChunkMath.GlobalToChunk((x, y, z + 1)).Xz].ChunkSections[i]
                                    .GetBlockId(ChunkMath.GlobalToLocal((x, y, z + 1))) == 0)
                            {
                                Config.Register.GetBlockFromId(id).BlockModel.AddFace(mesh.Data, Direction.Back, (x,y,z));
                            }
                            
                            if (world.ChunkColumns[column.Position + ChunkMath.GlobalToChunk((x - 1, y, z)).Xz].ChunkSections[i]
                                    .GetBlockId(ChunkMath.GlobalToLocal((x - 1, y, z))) == 0)
                            {
                                Config.Register.GetBlockFromId(id).BlockModel.AddFace(mesh.Data, Direction.Left, (x,y,z));
                            }
                            
                            if (world.ChunkColumns[column.Position + ChunkMath.GlobalToChunk((x+1, y, z)).Xz].ChunkSections[i]
                                    .GetBlockId(ChunkMath.GlobalToLocal((x+1, y, z))) == 0)
                            {
                                Config.Register.GetBlockFromId(id).BlockModel.AddFace(mesh.Data, Direction.Right, (x,y,z));
                            }
                        }
                    }
                }
            }
        }
        
        column.Status = ChunkStatus.Upload;
        UploadQueue.Enqueue(column.Position);
    }
    
    public void UploadMesh(Chunk column)
    {
        for (int i = 0; i < Config.ColumnSize; i++)
        {
            ChunkSectionMesh mesh = column.ChunkMeshes[i];
            if (!mesh.NeedsUpdates) continue;
            
            GL.DeleteVertexArray(mesh.Vao);
            GL.DeleteBuffer(mesh.Vbo);
            
            mesh.Vao = GL.GenVertexArray();
            GL.BindVertexArray(mesh.Vao);

            mesh.Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.Vbo);
            GL.BufferData<ChunkVertex>(BufferTarget.ArrayBuffer, Marshal.SizeOf<ChunkVertex>() * mesh.Data.Count, CollectionsMarshal.AsSpan(mesh.Data), BufferUsage.StaticDraw);
        
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.Position)));
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.Normal)));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.TextureCoordinate)));
            GL.EnableVertexAttribArray(2);
        
            // if (mesh.Length != mesh.Data.Count) Console.WriteLine($"previous length: {mesh.Length} new length: {mesh.Data.Count}");
            
            mesh.Length = mesh.Data.Count;
            mesh.Data.Clear();
            mesh.NeedsUpdates = false;
        }
        
        column.Status = ChunkStatus.Done;
    }

    public void EnqueueChunk(Vector2i position, ChunkStatus chunkStatus, bool hasPriority)
    {
        _world.ChunkColumns[position].Status = chunkStatus;
        GenerationQueue.Enqueue(position);
    }
}