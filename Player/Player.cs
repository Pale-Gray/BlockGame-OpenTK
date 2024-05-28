using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK.PlayerUtil
{
    internal class Player
    {

        Vector3 Position = Vector3.Zero;
        PlayerCamera Camera = new PlayerCamera();
        float CameraOffsetY = 0.0f;
        float WalkSpeed = 4.0f;
        float RunSpeed = 10.0f;

        public Player()
        {



        }

        public void UpdatePlayer()
        {

            if (Globals.Keyboard.IsKeyDown(Keys.E))
            {

                AddPlayerPosition((0,1,0));

            }

            if (Globals.Keyboard.IsKeyDown(Keys.Q))
            {

                AddPlayerPosition((0, -1, 0));

            }

            Camera.Update(Position, Vector3.UnitZ, Vector3.UnitY, 0, 0, 0);

        }

        public void SetHeight(float heightInBlocks)
        {


            CameraOffsetY = heightInBlocks;

        }

        public void SetPlayerPosition(Vector3 playerPosition)
        {

            Position = playerPosition;

        }

        public void SetRunSpeed(float runSpeedInBlocks)
        {

            RunSpeed = runSpeedInBlocks;

        }

        public void SetWalkSpeed(float speedInBlocks)
        {

            WalkSpeed = speedInBlocks;

        }

        public void AddPlayerPosition(Vector3 additionalPosition)
        {

            Position += additionalPosition;

        }

    }
}
