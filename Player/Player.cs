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
    public int CurrentSelectedIndex = 1;
    public PlayerChunkLoader Loader;

    public void UpdateInputs()
    {

        float speed = 1.0f;
        if (Input.IsKeyDown(Key.LeftControl)) speed = 10.0f;

        if (Input.IsKeyDown(Key.E))
        {
            Position.Y += (20.0f * speed) * (float)GlobalValues.DeltaTime;
            Camera.Position.Y = Position.Y;
            Camera.UpdateViewMatrix();
        }

        if (Input.IsKeyDown(Key.Q))
        {
            Position.Y -= (20.0f * speed) * (float)GlobalValues.DeltaTime;
            Camera.Position.Y = Position.Y;
            Camera.UpdateViewMatrix();
        }

        if (Input.IsKeyDown(Key.W))
        {
            Position -= (20.0f * speed) * new Vector3(Camera.FlattenedForwardVector.X, 0.0f, Camera.FlattenedForwardVector.Z) * (float) GlobalValues.DeltaTime;
            Camera.Position = Position;
            Camera.UpdateViewMatrix();
        }

        if (Input.IsKeyDown(Key.S))
        {
            Position += (20.0f * speed) * new Vector3(Camera.FlattenedForwardVector.X, 0.0f, Camera.FlattenedForwardVector.Z) * (float) GlobalValues.DeltaTime;
            Camera.Position = Position;
            Camera.UpdateViewMatrix();
        }

        if (Input.IsKeyDown(Key.A))
        {
            Position -= (20.0f * speed) * new Vector3(Camera.FlattenedLeftVector.X, 0.0f, Camera.FlattenedLeftVector.Z) * (float) GlobalValues.DeltaTime;
            Camera.Position = Position;
            Camera.UpdateViewMatrix();
        }

        if (Input.IsKeyDown(Key.D))
        {
            Position += (20.0f * speed) * new Vector3(Camera.FlattenedLeftVector.X, 0.0f, Camera.FlattenedLeftVector.Z) * (float) GlobalValues.DeltaTime;
            Camera.Position = Position;
            Camera.UpdateViewMatrix();
        }

    }

}