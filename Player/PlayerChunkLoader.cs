using System;
using System.Collections.Generic;
using System.Linq;
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

    private Vector2i _previousPlayerPosition;
    public Vector2i PlayerPosition;

    public PlayerChunkLoader(Vector2i playerPosition)
    {

        PlayerPosition = playerPosition;

    }

    public void Tick(World world, bool initialState = false)
    {

        if (Maths.ChebyshevDistance2D(_previousPlayerPosition, PlayerPosition) >= 2 || initialState)
        {

            _previousPlayerPosition = PlayerPosition;

            for (int x = -WorldGenerator.WorldGenerationRadius; x <= WorldGenerator.WorldGenerationRadius; x++)
            {

                for (int z = -WorldGenerator.WorldGenerationRadius; z <= WorldGenerator.WorldGenerationRadius; z++)
                {

                    if (GameState.World.WorldColumns.ContainsKey((x,z) + _previousPlayerPosition))
                    {

                        if (GameState.World.WorldColumns[(x,z) + _previousPlayerPosition].QueueType < QueueType.Done)
                        {

                            WorldGenerator.LowPriorityWorldGenerationQueue.Enqueue((x,z) + _previousPlayerPosition);

                        }

                    } else
                    {

                        GameState.World.WorldColumns.TryAdd((x,z) + _previousPlayerPosition, new ChunkColumn((x,z) + _previousPlayerPosition));
                        WorldGenerator.LowPriorityWorldGenerationQueue.Enqueue((x,z) + _previousPlayerPosition);

                    }

                }

            }

        }

    }

}