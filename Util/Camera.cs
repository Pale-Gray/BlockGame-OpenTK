using System;
using OpenTK.Mathematics;
using OpenTK.Platform;
using VoxelGame.Util;

namespace VoxelGame;

public enum CameraMode
{
    Perspective,
    Orthographic
}

public class Camera
{
    private float _fov = 60.0f;
    private int _width = 1920, _height = 1080;
    private CameraMode _mode = CameraMode.Perspective;
    
    public Matrix4 Projection;
    public Matrix4 View;

    public float Fov
    {
        get => _fov;
        set
        {
            _fov = value;
            ResetProjection();
        }
    }

    public int Width
    {
        get => _width;
        set
        {
            _width = value;
            ResetProjection();
        }
    }

    public int Height
    {
        get => _height;
        set
        {
            _height = value;
            ResetProjection();
        }
    }

    public CameraMode Mode
    {
        get => _mode;
        set
        {
            _mode = value;
            ResetProjection();
        }
    }
    
    public Camera(float fov, int width, int height, CameraMode mode)
    {
        Fov = fov;
        Width = width;
        Height = height;
        Mode = mode;
    }

    public void ResetProjection()
    {
        switch (Mode)
        {
            case CameraMode.Perspective:
                Projection = Matrix4.CreatePerspectiveFieldOfView(float.DegreesToRadians(Fov), (float) Width / Height, 0.1f, 1000.0f);
                break;
            case CameraMode.Orthographic:
                Projection = Matrix4.CreateOrthographic(Width, Height, 0.1f, 1000.0f);
                break;
        }
    }
}

public class MoveableCamera : Camera
{
    private Matrix4 _rotation;
    private Matrix4 _translation;
    private Matrix4 _positiveZForward = Matrix4.CreateScale(0, 0, -1);
    public Vector3 Position;
    public Vector3 Rotation;
    public float Speed = 25.0f;
    
    public MoveableCamera(float fov, int width, int height, CameraMode mode) : base(fov, width, height, mode)
    {
        
    }

    public void Poll()
    {
        Rotation.Y += Input.MouseDelta.X;
        Rotation.X += Input.MouseDelta.Y;
      
        if (Input.IsKeyDown(Key.S))
        {
            Position.Z -= (Speed * float.Cos(float.DegreesToRadians(Rotation.Y))) * Config.DeltaTime;
            Position.X -= (Speed * float.Sin(float.DegreesToRadians(Rotation.Y))) * Config.DeltaTime;
        }

        if (Input.IsKeyDown(Key.W))
        {
            Position.Z += (Speed * float.Cos(float.DegreesToRadians(Rotation.Y))) * Config.DeltaTime;
            Position.X += (Speed * float.Sin(float.DegreesToRadians(Rotation.Y))) * Config.DeltaTime;
        }

        if (Input.IsKeyDown(Key.A))
        {
            Position.X -= (Speed * float.Cos(float.DegreesToRadians(Rotation.Y))) * Config.DeltaTime;
            Position.Z += (Speed * float.Sin(float.DegreesToRadians(Rotation.Y))) * Config.DeltaTime;
        }

        if (Input.IsKeyDown(Key.D))
        {
            Position.X += (Speed * float.Cos(float.DegreesToRadians(Rotation.Y))) * Config.DeltaTime;
            Position.Z -= (Speed * float.Sin(float.DegreesToRadians(Rotation.Y))) * Config.DeltaTime;
        }

        if (Input.IsKeyDown(Key.E))
        {
            Position.Y += Speed * Config.DeltaTime;
        }

        if (Input.IsKeyDown(Key.Q))
        {
            Position.Y -= Speed * Config.DeltaTime;
        }

        _rotation = Matrix4.CreateRotationY(float.DegreesToRadians(Rotation.Y)) * Matrix4.CreateRotationX(float.DegreesToRadians(Rotation.X));
        _translation = Matrix4.CreateTranslation((-Position.X, -Position.Y, Position.Z));
        View = _translation * _rotation;
    }
}