using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using opentk_proj.util;

namespace opentk_proj
{
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
        public int cameraType = 0;
        public Camera(Vector3 position, Vector3 front, Vector3 up, int cameraType, float fov) {

            this.position = position;
            this.front = front;
            this.up = up;
            this.cameraType = cameraType;
            this.fov = fov;

            switch(cameraType)
            {

                case Orthographic:
                    projection = Matrix4.CreateOrthographic(Constants.WIDTH, Constants.HEIGHT, 0.1f, 100f);
                    break;
                case Perspective:
                    projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), (float)Constants.WIDTH / (float)Constants.HEIGHT, 0.1f, 100f);
                    break;
                default:
                    projection = Matrix4.CreateOrthographic(Constants.WIDTH, Constants.HEIGHT, 0.1f, 100f);
                    break;

            }

            // sets in case you dont use Update() but won't update the view matrix of course.
            view = Matrix4.LookAt(position, position + front, up);

        }

        public void Update(Vector3 position, Vector3 front, Vector3 up, float yaw, float pitch, float roll)
        {

            // front.X = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(yaw));
            // front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
            // front.Z = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(yaw));
            // front = Vector3.Normalize(front);

            view = Matrix4.LookAt(position, position + front, up);

        }

        public Matrix4 GetViewMatrix()
        {

            return view;

        }

    }
}
