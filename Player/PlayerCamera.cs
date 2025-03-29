using System;
using Game.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Game.PlayerUtil
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

        public Vector3 ForwardVector => (Matrix3.CreateRotationZ(Maths.ToRadians(RotationAxes.Z)) 
                                      * Matrix3.CreateRotationY(Maths.ToRadians(RotationAxes.Y))
                                      * Matrix3.CreateRotationX(Maths.ToRadians(RotationAxes.X))).Column2;// ((float) -Math.Sin(Maths.ToRadians(RotationAxes.Y)), 0, (float) Math.Cos(Maths.ToRadians(RotationAxes.Y)));

        public Vector3 LeftVector => (Matrix3.CreateRotationZ(Maths.ToRadians(RotationAxes.Z)) 
                                      * Matrix3.CreateRotationY(Maths.ToRadians(RotationAxes.Y))
                                      * Matrix3.CreateRotationX(Maths.ToRadians(RotationAxes.X))).Column0;

        public Vector3 FlattenedForwardVector => (
                                      Matrix3.CreateRotationY(Maths.ToRadians(RotationAxes.Y))
                                      ).Column2;

        public Vector3 FlattenedLeftVector => ( 
                                      Matrix3.CreateRotationY(Maths.ToRadians(RotationAxes.Y))
                                      ).Column0;
        public float FieldOfView;

        public PlayerCamera(float fov)
        {

            FieldOfView = fov;
            UpdateViewMatrix();
            Resize();

        }

        public PlayerCamera(float fov, Vector3 up, Vector3 left, Vector3 forwards)
        {

            FieldOfView = fov;
            UpdateViewMatrix();
            Resize();
            // just assume +X +Y +Z
            ConversionMatrix = Matrix4.CreateScale(-left + up + -forwards);

        }

        public void Update()
        {

            // if (Input.FocusAwareMouseDelta != Vector2.Zero) Console.WriteLine(Input.FocusAwareMouseDelta);

            RotationAxes.Y -= Input.FocusAwareMouseDelta.X;
            RotationAxes.X = Math.Clamp(RotationAxes.X - Input.FocusAwareMouseDelta.Y, -90, 90);
            UpdateViewMatrix();

        }

        public void UpdateViewMatrix()
        {

            ViewMatrix = Matrix4.CreateTranslation(Position).Inverted() * 
                         Matrix4.CreateRotationZ(Maths.ToRadians(RotationAxes.Z)) *
                         Matrix4.CreateRotationY(Maths.ToRadians(RotationAxes.Y)) * 
                         Matrix4.CreateRotationX(Maths.ToRadians(RotationAxes.X));

        }
        public void Resize()
        {

            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(Maths.ToRadians(FieldOfView), GlobalValues.Width / GlobalValues.Height, 0.1f, 1000.0f);

        }

    }
}
