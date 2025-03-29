using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Game.Core.Chunks;
using Game.Core.Worlds;
using Game.Util;
using OpenTK.Mathematics;

namespace Game.Core.PlayerUtil;

public class PlayerChunkLoader
{

    private HashSet<Vector2i> _existingChunks = new();
    private Queue<Vector2i> _samples = new();
    public Vector2i PlayerPosition;
    private Vector2i[] _neighbors = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    public PlayerChunkLoader(Vector2i playerPosition)
    {

        PlayerPosition = playerPosition;

    }

    public void Tick(World world)
    {

        int updates = 0;
        while (_samples.Count > 0 && updates < 5)
        {

            if (_samples.TryDequeue(out Vector2i sample))
            {

                foreach (Vector2i neighbor in _neighbors)
                {

                    if (Maths.ChebyshevDistance2D(sample + neighbor, PlayerPosition) <= WorldGenerator.WorldGenerationRadius && !_existingChunks.Contains(sample + neighbor))
                    {

                        _existingChunks.Add(sample + neighbor);
                        _samples.Enqueue(sample + neighbor);
                        // add a new chunk to the generation queue
                        world.WorldColumns.TryAdd(sample + neighbor, new ChunkColumn(sample + neighbor) { QueueType = ColumnQueueType.PassOne });
                        WorldGenerator.WorldGenerationQueue.Enqueue(sample + neighbor);

                    }

                }

            }
            updates++;

        }

    }

    public void QueuePosition(World world, Vector2i position)
    {

        _samples.Enqueue(position);
        if (!_existingChunks.Contains(position))
        {

            _existingChunks.Add(position);
            // add a new chunk to the generation queue 
            world.WorldColumns.TryAdd(position, new ChunkColumn(position) { QueueType = ColumnQueueType.PassOne });
            WorldGenerator.WorldGenerationQueue.Enqueue(position);

        }

    }

}