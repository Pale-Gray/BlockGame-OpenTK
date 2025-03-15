using System;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.PlayerUtil
{
    public class PlayerCamera
    {

        public Vector3 RotationAxes;
        public Vector3 Position;

        public Vector3 Up;
        public Vector3 Left;
        public Vector3 Forwards;

        private Vector3 _openGlUp = Vector3.UnitY;
        private Vector3 _openGlLeft = -Vector3.UnitX;
        private Vector3 _openGlForwards = -Vector3.UnitZ;

        public Matrix4 ViewMatrix { get; private set; }
        public Matrix4 ProjectionMatrix { get; private set; }
        public Matrix4 ConversionMatrix { get; private set; }

        public Vector3 ForwardVector => new Vector3(0, 0, 1) * Matrix3.CreateRotationZ(Maths.ToRadians(RotationAxes.Z)) *
                                                               Matrix3.CreateRotationY(Maths.ToRadians(RotationAxes.Y)) *
                                                               Matrix3.CreateRotationX(Maths.ToRadians(RotationAxes.X));
        public float FieldOfView;

        public PlayerCamera(float fov)
        {

            FieldOfView = fov;
            UpdateViewMatrix();
            UpdateProjectionMatrix();

        }

        public PlayerCamera(float fov, Vector3 up, Vector3 left, Vector3 forwards)
        {

            FieldOfView = fov;
            UpdateViewMatrix();
            UpdateProjectionMatrix();
            // just assume +X +Y +Z
            ConversionMatrix = Matrix4.CreateScale(-left + up + -forwards);

        }

        public void Update()
        {

            RotationAxes.Y -= Input.FocusAwareMouseDelta.X;
            RotationAxes.X = Math.Clamp(RotationAxes.X - Input.FocusAwareMouseDelta.Y, -89, 89);
            UpdateViewMatrix();

        }

        public void UpdateViewMatrix()
        {

            ViewMatrix = Matrix4.CreateTranslation(Position).Inverted() * 
                         Matrix4.CreateRotationZ(Maths.ToRadians(RotationAxes.Z)) *
                         Matrix4.CreateRotationY(Maths.ToRadians(RotationAxes.Y)) * 
                         Matrix4.CreateRotationX(Maths.ToRadians(RotationAxes.X));

        }
        public void UpdateProjectionMatrix()
        {

            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(Maths.ToRadians(FieldOfView), GlobalValues.Width / GlobalValues.Height, 0.1f, 1000.0f);

        }

    }
}
