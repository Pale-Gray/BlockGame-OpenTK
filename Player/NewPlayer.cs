using System.Collections.Generic;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.PlayerUtil;

public class NewPlayer
{

    public int NetPeerId;
    public long UserId;
    public string DisplayName;
    public Vector3 Position;

    public HashSet<Vector2i> ChunkArea;

    public void ResolveAreaDifference(Vector2i playerChunkPosition)
    {

        if (ChunkArea == null)
        {
            ChunkArea = new();
            for (int x = -PackedWorldGenerator.WorldGenerationRadius; x <= PackedWorldGenerator.WorldGenerationRadius; x++)
            {
                for (int z = -PackedWorldGenerator.WorldGenerationRadius; z <= PackedWorldGenerator.WorldGenerationRadius; z++)
                {
                    ChunkArea.Add((x,z) + playerChunkPosition);
                    if (NetworkingValues.Server.World.WorldColumns.ContainsKey((x,z) + playerChunkPosition))
                    {
                        NetworkingValues.Server.World.WorldColumns[(x,z) + playerChunkPosition].QueueType = ColumnQueueType.PassOne;
                        PackedWorldGenerator.ColumnWorldGenerationQueue.EnqueueLast((x,z) + playerChunkPosition);
                        // call a send method.
                    } else
                    {
                        NetworkingValues.Server.World.WorldColumns.TryAdd((x,z) + playerChunkPosition, new ChunkColumn((x,z) + playerChunkPosition));
                        PackedWorldGenerator.ColumnWorldGenerationQueue.EnqueueLast((x,z) + playerChunkPosition);
                        // call the generate method that eventually sends it.
                    }
                }
            }
        } else
        {

            HashSet<Vector2i> newPositions = new();

            foreach (Vector2i chunkPos in ChunkArea)
            {
                newPositions.Add(chunkPos + playerChunkPosition);
            }

            foreach (Vector2i chunkPos in newPositions)
            {

                // doesnt contain new chunk positions
                if (!ChunkArea.Contains(chunkPos))
                {

                    if (NetworkingValues.Server.World.WorldColumns.ContainsKey(chunkPos + playerChunkPosition))
                    {
                        // call a send method.
                    } else
                    {
                        // call the generate method that eventually sends it.
                    }

                }

            }

            foreach (Vector2i chunkPos in ChunkArea)
            {

                // contains chunks that need to be removed from the client / saved and removed from the server.
                if (!newPositions.Contains(chunkPos))
                {

                    // call a remove method on the client
                    // if valid, call a remove method on the server (given no players have the chunk)

                }

            }

        }

    }

}