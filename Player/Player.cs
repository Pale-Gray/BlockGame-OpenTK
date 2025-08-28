using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Platform;
using VoxelGame.Util;

namespace VoxelGame;

public class Player : Entity
{
    public string Name;
    public HashSet<Vector2i> LoadedChunks = new();
    public MoveableCamera Camera;
    public float Speed = 20.0f;

    public Player(string name)
    {
        Name = name;
        Camera = new MoveableCamera(90.0f, Config.Width, Config.Height, CameraMode.Perspective);
    }

    public override void TickUpdate(World world)
    {
        base.TickUpdate(world);
        
        if (Input.IsKeyDown(Key.S))
        {
            Position.Z -= (Speed * float.Cos(float.DegreesToRadians(Camera.Rotation.Y))) * Config.TickRate;
            Position.X -= (Speed * float.Sin(float.DegreesToRadians(Camera.Rotation.Y))) * Config.TickRate;
        }

        if (Input.IsKeyDown(Key.W))
        {
            Position.Z += (Speed * float.Cos(float.DegreesToRadians(Camera.Rotation.Y))) * Config.TickRate;
            Position.X += (Speed * float.Sin(float.DegreesToRadians(Camera.Rotation.Y))) * Config.TickRate;
        }

        if (Input.IsKeyDown(Key.A))
        {
            Position.X -= (Speed * float.Cos(float.DegreesToRadians(Camera.Rotation.Y))) * Config.TickRate;
            Position.Z += (Speed * float.Sin(float.DegreesToRadians(Camera.Rotation.Y))) * Config.TickRate;
        }

        if (Input.IsKeyDown(Key.D))
        {
            Position.X += (Speed * float.Cos(float.DegreesToRadians(Camera.Rotation.Y))) * Config.TickRate;
            Position.Z -= (Speed * float.Sin(float.DegreesToRadians(Camera.Rotation.Y))) * Config.TickRate;
        }

        if (Input.IsKeyDown(Key.E))
        {
            Position.Y += Speed * Config.TickRate;
        }

        if (Input.IsKeyDown(Key.Q))
        {
            Position.Y -= Speed * Config.TickRate;
        }
    }

    public override void Update()
    {
        Camera.UpdateRotation();
        Camera.Position = Vector3.Lerp(PreviousPosition, Position, Config.TickInterpolant);
    }
}