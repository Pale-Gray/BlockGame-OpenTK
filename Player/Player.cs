using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using Blockgame_OpenTK.Util;
using System;
using Blockgame_OpenTK.ChunkUtil;
using Blockgame_OpenTK.BlockUtil;

namespace Blockgame_OpenTK.PlayerUtil
{
    internal class Player
    {

        Vector3 Position = Vector3.Zero;
        public Camera Camera = new Camera(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, CameraType.Perspective, 90.0f);
        float CameraOffsetY = 0.0f;
        float WalkSpeed = 15.0f;
        float RunSpeed = 50.0f;

        float PressDelay = 1.0f;

        public Player() { }

        public void Update()
        {
            float Speed = 0;
            // Console.WriteLine("player delta: " + Globals.DeltaTime);
            if (Globals.Keyboard.IsKeyDown(Keys.LeftShift))
            {

                Speed = RunSpeed;

            } else
            {

                Speed = WalkSpeed;

            }

            if (Globals.Mouse.IsButtonDown(MouseButton.Left))
            {

                PressDelay += (float) Globals.DeltaTime;

                if (PressDelay >= 0.5f)
                {

                    // GetBlockLooking(10);
                    // Console.WriteLine(DDA.TraceChunks(ChunkLoader.ChunkDictionary, Camera.Position, Camera.ForwardVector, 10));
                    // Console.WriteLine("campos: {0}", Camera.Position);
                    DDA.TraceChunks(ChunkLoader.ChunkDictionary, Camera.Position, Camera.ForwardVector, 5);
                    // ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(DDA.HitChunkPosition)).SetBlockRewrite(Blocks.Air, (Vector3i) DDA.HitBlockPositionLocalToChunk);
                    Vector3i HitPositionLocal = (Vector3i)ChunkUtils.PositionToBlockLocal(DDA.PositionAtHit);

                    if (ChunkLoader.ContainsGeneratedChunk(DDA.ChunkAtHit))
                    {

                        ChunkLoader.GetChunk(DDA.ChunkAtHit).SetBlockRewrite(Blocks.Air, HitPositionLocal);
                        if (HitPositionLocal.X == 0) { ChunkLoader.GetChunk(DDA.ChunkAtHit - (1, 0, 0)).Remesh(); }
                        if (HitPositionLocal.X == Globals.ChunkSize - 1) { ChunkLoader.GetChunk(DDA.ChunkAtHit + (1, 0, 0)).Remesh(); }
                        if (HitPositionLocal.Y == 0) { ChunkLoader.GetChunk(DDA.ChunkAtHit - (0, 1, 0)).Remesh(); }
                        if (HitPositionLocal.Y == Globals.ChunkSize - 1) { ChunkLoader.GetChunk(DDA.ChunkAtHit + (0, 1, 0)).Remesh(); }
                        if (HitPositionLocal.Z == 0) { ChunkLoader.GetChunk(DDA.ChunkAtHit - (0, 0, 1)).Remesh(); }
                        if (HitPositionLocal.Z == Globals.ChunkSize - 1) { ChunkLoader.GetChunk(DDA.ChunkAtHit + (0, 0, 1)).Remesh(); }

                    }

                    PressDelay = 0;

                }

            }

            if (Globals.Mouse.IsButtonDown(MouseButton.Right))
            {

                PressDelay += (float)Globals.DeltaTime;

                if (PressDelay >= 0.5f)
                {

                    // GetBlockLooking(10);
                    // Console.WriteLine(DDA.TraceChunks(ChunkLoader.ChunkDictionary, Camera.Position, Camera.ForwardVector, 10));
                    // Console.WriteLine("campos: {0}", Camera.Position);
                    DDA.TraceChunks(ChunkLoader.ChunkDictionary, Camera.Position, Camera.ForwardVector, 5);
                    // ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(DDA.HitChunkPosition)).SetBlockRewrite(Blocks.Air, (Vector3i) DDA.HitBlockPositionLocalToChunk);
                    Vector3i HitPositionLocal = (Vector3i)ChunkUtils.PositionToBlockLocal(DDA.PositionAtHit);
                    // Console.WriteLine(DDA.FaceHit);
                    // Console.WriteLine(((DDA.SmoothPosition.X+0.5f) % 1 - 0.5f, (DDA.SmoothPosition.Y+0.5f) % 1 - 0.5f, (DDA.SmoothPosition.Z+0.5f) % 1 - 0.5f));

                    Vector3 hitvec = 2 * (DDA.SmoothPosition - ((Vector3)DDA.PositionAtHit + (0.5f, 0.5f, 0.5f)));
                    // Console.WriteLine(hitvec);
                    Vector3i BuildOffset = Vector3i.Zero;
                    if (hitvec.X >= 1 - float.Epsilon) { BuildOffset.X = 1; }
                    if (hitvec.Y >= 1 - float.Epsilon) { BuildOffset.Y = 1; }
                    if (hitvec.Z >= 1 - float.Epsilon) { BuildOffset.Z = 1; }
                    if (hitvec.X <= -1 + float.Epsilon) { BuildOffset.X = -1; }
                    if (hitvec.Y <= -1 + float.Epsilon) { BuildOffset.Y = -1; }
                    if (hitvec.Z <= -1 + float.Epsilon) { BuildOffset.Z = -1; }
                    // Console.WriteLine(BuildOffset);
                    Console.WriteLine("smoothpos: {0}, positionhit: {1}, hitvec: {2}, buildOffset: {3}", DDA.SmoothPosition, (Vector3)DDA.PositionAtHit + (0.5f,0.5f,0.5f), hitvec, BuildOffset);
                    // Console.WriteLine((Math.Round(hitvec.X), Math.Round(hitvec.Y), Math.Round(hitvec.Z)));

                    Vector3i BuildCoordinate = HitPositionLocal + BuildOffset;

                    if (ChunkLoader.ContainsGeneratedChunk(ChunkUtils.PositionToChunk(DDA.PositionAtHit + BuildOffset)))
                    {
                        // Console.WriteLine("yes");
                        ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(DDA.PositionAtHit + BuildOffset)).SetBlockRewrite(Blocks.Stone, (Vector3i)ChunkUtils.PositionToBlockLocal(DDA.PositionAtHit + BuildOffset));

                    }

                    PressDelay = 0;

                }

            }
            else
            {

                PressDelay = 0.5f;

            }

            if (Globals.Keyboard.IsKeyDown(Keys.W))
            {

                Position += Camera.ForwardVector * Speed * (float)Globals.DeltaTime;

            }
            if (Globals.Keyboard.IsKeyDown(Keys.A))
            {

                Position -= Vector3.Normalize(Vector3.Cross(Camera.ForwardVector, Camera.UpVector)) * (Speed * (float)Globals.DeltaTime);

            }
            if (Globals.Keyboard.IsKeyDown(Keys.S))
            {

                Position -= Camera.ForwardVector * Speed * (float)Globals.DeltaTime;

            }
            if (Globals.Keyboard.IsKeyDown(Keys.D))
            {

                Position += Vector3.Normalize(Vector3.Cross(Camera.ForwardVector, Camera.UpVector)) * (Speed * (float)Globals.DeltaTime);

            }

            if (Globals.Keyboard.IsKeyDown(Keys.E))
            {

                AddPlayerPosition(Camera.UpVector * (Speed * (float)Globals.DeltaTime));

            }

            if (Globals.Keyboard.IsKeyDown(Keys.Q))
            {

                AddPlayerPosition(Camera.UpVector * (-Speed * (float)Globals.DeltaTime));

            }

            Camera.Update(Position + (0,CameraOffsetY,0));

        }

        public void GetBlockLooking(int range)
        {

            DDA.Trace(ChunkLoader.ChunkDictionary, Camera.Position, Camera.ForwardVector, range);

        }

        public void SetHeight(float heightInBlocks)
        {

            CameraOffsetY = heightInBlocks;

        }

        public void SetPosition(Vector3 playerPosition)
        {

            Position = playerPosition;
            Camera.SetPosition(playerPosition);

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
