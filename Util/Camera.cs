using OpenTK.Mathematics;
using System;

namespace Blockgame_OpenTK.Util
{
    public enum CameraType
    {

        Orthographic,
        Perspective

    };

    public class Camera
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

            UpdateProjectionMatrix();

            // sets in case you dont use Update() but won't update the view matrix of course.
            ViewMatrix = Matrix4.LookAt(position, position + forwards, up);

        }

        public void UpdateProjectionMatrix()
        {

            switch (CameraType)
            {

                case CameraType.Orthographic:
                    ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, GlobalValues.Width, GlobalValues.Height, 0, 0.1f, 1000f);
                    break;
                case CameraType.Perspective:
                    ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), GlobalValues.Width / GlobalValues.Height, 0.1f, 100000f);
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

            Vector2 MouseDelta = -Input.FocusAwareMouseDelta;

            Yaw += MouseDelta.X * GlobalValues.Settings.MouseSensitivity;
            Pitch -= MouseDelta.Y * GlobalValues.Settings.MouseSensitivity;

            // Console.WriteLine(Input.JoystickRightAxis);
            // Yaw += Input.JoystickRightAxis.Y * GlobalValues.Settings.MouseSensitivity;
            // Pitch += Input.JoystickRightAxis.X * GlobalValues.Settings.MouseSensitivity;
            // Pitch = Math.Clamp(Pitch, -88, 88);

            CalculateFrontFromYawPitch(Yaw, Pitch);
            
            Position = position;

            ViewMatrix = Matrix4.LookAt(Position, Position + ForwardVector, UpVector);


        }

        public void CalculateFrontFromYawPitch(float yaw, float pitch)
        {

            ForwardVector.X = (float)Math.Cos(Maths.ToRadians(pitch)) * (float)Math.Cos(Maths.ToRadians(yaw));
            ForwardVector.Y = (float)Math.Sin(Maths.ToRadians(pitch));
            ForwardVector.Z = (float)Math.Cos(Maths.ToRadians(pitch)) * (float)Math.Sin(Maths.ToRadians(yaw));
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
