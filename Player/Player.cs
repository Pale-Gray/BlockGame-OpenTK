using System.Collections.Generic;
using Game.Core.Chunks;
using Game.Core.Worlds;
using Game.PlayerUtil;
using Game.Util;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace Game.Core.PlayerUtil;

public class Player
{

    public int NetPeerId;
    public long UserId;
    public string DisplayName;
    public Vector3 Position;
    public HashSet<Vector2i> ChunkArea = new();
    public HashSet<Vector2i> SentChunks = new();
    public PlayerCamera Camera = new PlayerCamera(90.0f, Vector3.UnitY, Vector3.UnitX, Vector3.UnitZ);
    public PlayerChunkLoader Loader;

    public void UpdateInputs()
    {

        if (Input.IsKeyDown(Key.E))
        {
            Camera.Position.Y += 20 * (float)GlobalValues.DeltaTime;
            Camera.UpdateViewMatrix();
        }

        if (Input.IsKeyDown(Key.Q))
        {
            Camera.Position.Y -= 20 * (float)GlobalValues.DeltaTime;
            Camera.UpdateViewMatrix();
        }

        if (Input.IsKeyDown(Key.W))
        {
            Camera.Position -= 20.0f * new Vector3(Camera.FlattenedForwardVector.X, 0.0f, Camera.FlattenedForwardVector.Z) * (float) GlobalValues.DeltaTime;
            Camera.UpdateViewMatrix();
        }

        if (Input.IsKeyDown(Key.S))
        {
            Camera.Position += 20 * new Vector3(Camera.FlattenedForwardVector.X, 0.0f, Camera.FlattenedForwardVector.Z) * (float) GlobalValues.DeltaTime;
            Camera.UpdateViewMatrix();
        }

        if (Input.IsKeyDown(Key.A))
        {
            Camera.Position -= 20 * new Vector3(Camera.FlattenedLeftVector.X, 0.0f, Camera.FlattenedLeftVector.Z) * (float) GlobalValues.DeltaTime;
            Camera.UpdateViewMatrix();
        }

        if (Input.IsKeyDown(Key.D))
        {
            Camera.Position += 20 * new Vector3(Camera.FlattenedLeftVector.X, 0.0f, Camera.FlattenedLeftVector.Z) * (float) GlobalValues.DeltaTime;
            Camera.UpdateViewMatrix();
        }

    }
    public void ResolveAreaDifference(Vector2i playerChunkPosition)
    {

        if (ChunkArea.Count == 0)
        {
            for (int x = -WorldGenerator.WorldGenerationRadius; x <= WorldGenerator.WorldGenerationRadius; x++)
            {
                for (int z = -WorldGenerator.WorldGenerationRadius; z <= WorldGenerator.WorldGenerationRadius; z++)
                {
                    ChunkArea.Add((x,z) + playerChunkPosition);
                    // if (NetworkingValues.Server.World.WorldColumns.ContainsKey((x,z) + playerChunkPosition))
                    // {
                    //     NetworkingValues.Server.World.WorldColumns[(x,z) + playerChunkPosition].QueueType = ColumnQueueType.PassOne;
                    //     PackedWorldGenerator.ColumnWorldGenerationQueue.EnqueueLast((x,z) + playerChunkPosition);
                    //     // call a send method.
                    // } else
                    // {
                    //     NetworkingValues.Server.World.WorldColumns.TryAdd((x,z) + playerChunkPosition, new ChunkColumn((x,z) + playerChunkPosition));
                    //     PackedWorldGenerator.ColumnWorldGenerationQueue.EnqueueLast((x,z) + playerChunkPosition);
                    //     // call the generate method that eventually sends it.
                    // }
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

                    // if (NetworkingValues.Server.World.WorldColumns.ContainsKey(chunkPos + playerChunkPosition))
                    {
                        // call a send method.
                    }//  else
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