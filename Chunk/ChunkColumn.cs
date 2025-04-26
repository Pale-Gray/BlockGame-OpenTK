using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Game.Core.BlockStorage;
using Game.Core.Worlds;
using Game.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Game.Core.Chunks;

public enum QueueType : byte
{

    PassOne,
    SunlightCalculation,
    LightPropagation,
    Mesh,
    Upload,
    Done,
    Unload

}
public class ChunkColumn
{

    public Vector2i Position;
    public Chunk[] Chunks = new Chunk[WorldGenerator.WorldGenerationHeight];
    public ChunkMesh[] ChunkMeshes = new ChunkMesh[WorldGenerator.WorldGenerationHeight];
    public int[] SolidHeightmap = new int[GlobalValues.ChunkSize * GlobalValues.ChunkSize];
    public Dictionary<Vector3i, IBlockProperties> BlockProperties = new();
    public Dictionary<Vector3i, (uint time, uint rate)> BlockTickers = new();
    public QueueType QueueType = QueueType.PassOne;
    public bool HasPriority = false;
    public bool IsUpdating = false;
    public ConcurrentQueue<Vector3i> SunlightAdditionQueue = new();
    public ConcurrentQueue<(Vector3i position, ushort value)> SunlightRemovalQueue = new();
    public ConcurrentQueue<Vector3i> RedBlocklightAdditionQueue = new();
    public ConcurrentQueue<(Vector3i position, ushort value)> RedBlocklightRemovalQueue = new();
    public ConcurrentQueue<Vector3i> GreenBlocklightAdditionQueue = new();
    public ConcurrentQueue<(Vector3i position, ushort value)> GreenBlocklightRemovalQueue = new();
    public ConcurrentQueue<Vector3i> BlueBlocklightAdditionQueue = new();
    public ConcurrentQueue<(Vector3i position, ushort value)> BlueBlocklightRemovalQueue = new();

    public ChunkColumn(Vector2i position)
    {
        Position = position;
        for (int i = 0; i < WorldGenerator.WorldGenerationHeight; i++)
        {
            Chunks[i] = new Chunk((position.X, i, position.Y));
            ChunkMeshes[i] = new ChunkMesh((position.X, i, position.Y));
        }
    }

    public void FreeResources()
    {

        for (int i = 0; i < WorldGenerator.WorldGenerationHeight; i++)
        {

            ChunkMeshes[i].Solids.Clear();
            ChunkMeshes[i].Cutouts.Clear();
            ChunkMeshes[i].SolidIndices.Clear();
            ChunkMeshes[i].CutoutIndices.Clear();

            GL.DeleteVertexArray(ChunkMeshes[i].SolidsVao);
            GL.DeleteBuffer(ChunkMeshes[i].SolidsHandle);
            GL.DeleteBuffer(ChunkMeshes[i].SolidsIbo);

            GL.DeleteVertexArray(ChunkMeshes[i].CutoutVao);
            GL.DeleteBuffer(ChunkMeshes[i].CutoutHandle);
            GL.DeleteBuffer(ChunkMeshes[i].CutoutIbo);

        }

    }
    
}