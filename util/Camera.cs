using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Util
{
    public enum CameraType
    {

        Orthographic,
        Perspective

    };
    internal class Camera
    {

        public Vector3 position;
        public Vector3 up;
        public Vector3 front;

        public Matrix4 projection;
        public Matrix4 view;

        public float yaw;
        public float pitch;
        public float roll;
        public float fov;

        public const int Orthographic = 0;
        public const int Perspective = 1;
        public CameraType type;

        public Camera(Vector3 position, Vector3 front, Vector3 up, CameraType type, float fov)
        {

            this.position = position;
            this.front = front;
            this.up = up;
            this.type = type;
            this.fov = fov;

            switch (type)
            {

                case CameraType.Orthographic:
                    projection = Matrix4.CreateOrthographic(Globals.WIDTH, Globals.HEIGHT, 0.1f, 1000f);
                    break;
                case CameraType.Perspective:
                    projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), Globals.WIDTH / Globals.HEIGHT, 0.1f, 1000f);
                    break;
                default:
                    projection = Matrix4.CreateOrthographic(Globals.WIDTH, Globals.HEIGHT, 0.1f, 1000f);
                    break;

            }

            // sets in case you dont use Update() but won't update the view matrix of course.
            view = Matrix4.LookAt(position, position + front, up);

        }

        public void UpdateProjectionMatrix()
        {

            switch (type)
            {

                case CameraType.Orthographic:
                    projection = Matrix4.CreateOrthographic(Globals.WIDTH, Globals.HEIGHT, 0.1f, 1000f);
                    break;
                case CameraType.Perspective:
                    projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), Globals.WIDTH / Globals.HEIGHT, 0.1f, 1000f);
                    break;
                default:
                    projection = Matrix4.CreateOrthographic(Globals.WIDTH, Globals.HEIGHT, 0.1f, 1000f);
                    break;

            }

        }
        public void Update(Vector3 position, Vector3 front, Vector3 up, float yaw, float pitch, float roll)
        {

            // front.X = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(yaw));
            // front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
            // front.Z = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(yaw));
            // front = Vector3.Normalize(front);

            this.front = front;
            this.position = position;
            this.up = up;

            view = Matrix4.LookAt(position, position + front, up);

        }

        public Matrix4 GetViewMatrix()
        {

            return view;

        }

    }
}
