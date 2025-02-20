using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.PlayerUtil
{
    public class PlayerCamera : Camera
    {
        public PlayerCamera() : base((0,0,0), (0,0,0), (0,0,0), CameraType.Perspective, 90.0f)
        {

            

        }

        public void Update(Vector3 position)
        {

            Vector2 MouseDelta = Input.MouseDelta;

            Yaw += MouseDelta.X;
            Pitch -= MouseDelta.Y;

            CalculateFrontFromYawPitch(Yaw, Pitch);
            Update(position, ForwardVector, UpVector);

        }

    }
}
