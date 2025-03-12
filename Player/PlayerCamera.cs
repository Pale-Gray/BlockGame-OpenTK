using System;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.PlayerUtil
{
    public class PlayerCamera
    {

        public Vector3 RotationAxes;
        public Vector3 Position;

        public Matrix4 ViewMatrix { get; private set; }
        public Matrix4 ProjectionMatrix { get; private set; }

        public Vector3 ForwardVector => new Vector3(0, 0, 1) * Matrix3.CreateRotationX(Maths.ToRadians(RotationAxes.X)) *
                                                               Matrix3.CreateRotationY(Maths.ToRadians(RotationAxes.Y)) *
                                                               Matrix3.CreateRotationZ(Maths.ToRadians(RotationAxes.Z));
        public float FieldOfView;

        public PlayerCamera(float fov)
        {

            FieldOfView = fov;
            UpdateViewMatrix();
            UpdateProjectionMatrix();

        }

        public void UpdateViewMatrix()
        {

            ViewMatrix = Matrix4.CreateRotationX(Maths.ToRadians(RotationAxes.X)) * 
                         Matrix4.CreateRotationY(Maths.ToRadians(RotationAxes.Y)) *
                         Matrix4.CreateRotationZ(Maths.ToRadians(RotationAxes.Z)) *
                         Matrix4.CreateTranslation(Position).Inverted();

        }
        public void UpdateProjectionMatrix()
        {

            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(Maths.ToRadians(FieldOfView), GlobalValues.Width / GlobalValues.Height, 0.1f, 1000.0f);

        }

    }
}
