﻿using OpenTK.Mathematics;
using System;

namespace Blockgame_OpenTK.Util
{
    public enum CameraType
    {

        Orthographic,
        Perspective

    };
    internal class Camera
    {

        public Vector3 Position;
        public Vector3 UpVector;
        public Vector3 ForwardVector;

        public Matrix4 ProjectionMatrix;
        public Matrix4 ViewMatrix;

        public float Fov = 90;
        public CameraType CameraType;

        public float Yaw = 0;
        public float Pitch = 0;
        public float Roll = 0;

        public Camera(Vector3 position, Vector3 forwards, Vector3 up, CameraType type, float fov)
        {

            Position = position;
            ForwardVector = forwards;
            UpVector = up;
            CameraType = type;
            Fov = fov;

            switch (CameraType)
            {

                case CameraType.Orthographic:
                    ProjectionMatrix = Matrix4.CreateOrthographic(Globals.WIDTH, Globals.HEIGHT, 0.1f, 1000f);
                    break;
                case CameraType.Perspective:
                    ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), Globals.WIDTH / Globals.HEIGHT, 0.1f, 1000f);
                    break;
                default:
                    ProjectionMatrix = Matrix4.CreateOrthographic(Globals.WIDTH, Globals.HEIGHT, 0.1f, 1000f);
                    break;

            }

            // sets in case you dont use Update() but won't update the view matrix of course.
            ViewMatrix = Matrix4.LookAt(position, position + forwards, up);

        }

        public void UpdateProjectionMatrix()
        {

            switch (CameraType)
            {

                case CameraType.Orthographic:
                    ProjectionMatrix = Matrix4.CreateOrthographic(Globals.WIDTH, Globals.HEIGHT, 0.1f, 1000f);
                    break;
                case CameraType.Perspective:
                    ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), Globals.WIDTH / Globals.HEIGHT, 0.1f, 1000f);
                    break;
                default:
                    ProjectionMatrix = Matrix4.CreateOrthographic(Globals.WIDTH, Globals.HEIGHT, 0.1f, 1000f);
                    break;

            }

        }
        public void Update(Vector3 position, Vector3 forwards, Vector3 up)
        {


            Position = position;
            ForwardVector = forwards;
            UpVector = up;

            ViewMatrix = Matrix4.LookAt(position, position + forwards, up);

        }

        public void Update(Vector3 position)
        {

            Vector2 MouseDelta = Globals.Mouse.Delta;

            Yaw += MouseDelta.X;
            Pitch -= MouseDelta.Y;

            CalculateFrontFromYawPitch(Yaw, Pitch);
            
            Position = position;

            ViewMatrix = Matrix4.LookAt(Position, Position + ForwardVector, UpVector);


        }

        public void CalculateFrontFromYawPitch(float yaw, float pitch)
        {

            ForwardVector.X = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(yaw));
            ForwardVector.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
            ForwardVector.Z = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(yaw));
            ForwardVector = Vector3.Normalize(ForwardVector);
        }

        public void SetPosition(Vector3 position)
        {

            Position = position;

        }

        public void SetFov(float fov)
        {

            Fov = fov;

        }

    }
}
