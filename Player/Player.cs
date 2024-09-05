using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using Blockgame_OpenTK.Util;
using System;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.BlockUtil;
using OpenTK.Windowing.Common;
using Blockgame_OpenTK.Core.Worlds;

namespace Blockgame_OpenTK.PlayerUtil
{
    internal class Player
    {

        public Vector3 Position = Vector3.Zero;
        public Camera Camera = new Camera(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, CameraType.Perspective, 90.0f);
        float CameraOffsetY = 0.0f;
        float WalkSpeed = 15.0f;
        float RunSpeed = 30.0f; // 1.0f;

        float PressDelay = 1.0f;
        float PlaceDelay = 1.0f;
        float RemoveDelay = 1.0f;

        public Player() { }

        public void Update(World world)
        {
            float Speed = 0;

            if (GlobalValues.CursorState == CursorState.Grabbed)
            {

                if (GlobalValues.Keyboard.IsKeyDown(Keys.LeftShift))
                {

                    Speed = RunSpeed;

                }
                else
                {

                    Speed = WalkSpeed;

                }

                if (GlobalValues.Mouse.IsButtonDown(MouseButton.Left))
                {

                    RemoveDelay += (float)GlobalValues.DeltaTime;

                    if (RemoveDelay >= 0.15f)
                    {

                        Dda.TraceChunks(world.WorldChunks, Camera.Position, Camera.ForwardVector, GlobalValues.PlayerRange);
                        if (Dda.hit)
                        {

                            Vector3i HitPositionLocal = (Vector3i)ChunkUtils.PositionToBlockLocal(Dda.PositionAtHit);

                            world.WorldChunks[Dda.ChunkAtHit].SetBlock(HitPositionLocal, Blocks.AirBlock);
                            WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit);

                            if (HitPositionLocal.Y == 0) WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit - Vector3i.UnitY);
                            if (HitPositionLocal.Y == GlobalValues.ChunkSize - 1) WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit + Vector3i.UnitY);
                            if (HitPositionLocal.X == 0) WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit - Vector3i.UnitX);
                            if (HitPositionLocal.X == GlobalValues.ChunkSize - 1) WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit + Vector3i.UnitX);
                            if (HitPositionLocal.Z == 0) WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit - Vector3i.UnitZ);
                            if (HitPositionLocal.Z == GlobalValues.ChunkSize - 1) WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit + Vector3i.UnitZ);

                            RemoveDelay = 0;

                        }
                        
                    }

                } else { RemoveDelay = 1; }

                if (GlobalValues.Mouse.IsButtonDown(MouseButton.Right))
                {

                    PlaceDelay += (float)GlobalValues.DeltaTime;

                    if (PlaceDelay >= 0.15f)
                    {

                        Dda.TraceChunks(world.WorldChunks, Camera.Position, Camera.ForwardVector, GlobalValues.PlayerRange);
                        
                        if (Dda.hit)
                        {

                            Vector3i HitPositionLocal = (Vector3i)ChunkUtils.PositionToBlockLocal(Dda.PreviousPositionAtHit);
                            Console.WriteLine("hitpos: {0}, prevpos: {1}", Dda.PositionAtHit, Dda.PreviousPositionAtHit);

                            if (Dda.PreviousPositionAtHit != ChunkUtils.PositionToBlockGlobal(Camera.Position))
                            {

                                Vector3i previousPositionChunkHit = ChunkUtils.PositionToChunk(Dda.PreviousPositionAtHit);

                                world.WorldChunks[previousPositionChunkHit].SetBlock(HitPositionLocal, Blocks.StoneBlock);
                                WorldGenerator.ChunkAlterUpdateQueue.Enqueue(previousPositionChunkHit);

                                if (HitPositionLocal.Y == 0) WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit - Vector3i.UnitY);
                                if (HitPositionLocal.Y == GlobalValues.ChunkSize - 1) WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit + Vector3i.UnitY);
                                if (HitPositionLocal.X == 0) WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit - Vector3i.UnitX);
                                if (HitPositionLocal.X == GlobalValues.ChunkSize - 1) WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit + Vector3i.UnitX);
                                if (HitPositionLocal.Z == 0) WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit - Vector3i.UnitZ);
                                if (HitPositionLocal.Z == GlobalValues.ChunkSize - 1) WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit + Vector3i.UnitZ);

                                WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit + Vector3i.UnitZ + Vector3i.UnitX);
                                WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit - Vector3i.UnitZ + Vector3i.UnitX);
                                WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit + Vector3i.UnitZ - Vector3i.UnitX);
                                WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit + Vector3i.UnitZ + Vector3i.UnitX);

                            }

                            PlaceDelay = 0;

                        }

                    }

                } else {  PlaceDelay = 1; }

                if (GlobalValues.Keyboard.IsKeyDown(Keys.W))
                {

                    Position += Camera.ForwardVector * Speed * (float)GlobalValues.DeltaTime;

                }
                if (GlobalValues.Keyboard.IsKeyDown(Keys.A))
                {

                    Position -= Vector3.Normalize(Vector3.Cross(Camera.ForwardVector, Camera.UpVector)) * (Speed * (float)GlobalValues.DeltaTime);

                }
                if (GlobalValues.Keyboard.IsKeyDown(Keys.S))
                {

                    Position -= Camera.ForwardVector * Speed * (float)GlobalValues.DeltaTime;

                }
                if (GlobalValues.Keyboard.IsKeyDown(Keys.D))
                {

                    Position += Vector3.Normalize(Vector3.Cross(Camera.ForwardVector, Camera.UpVector)) * (Speed * (float)GlobalValues.DeltaTime);

                }

                if (GlobalValues.Keyboard.IsKeyDown(Keys.E))
                {

                    AddPlayerPosition(Camera.UpVector * (Speed * (float)GlobalValues.DeltaTime));

                }

                if (GlobalValues.Keyboard.IsKeyDown(Keys.Q))
                {

                    AddPlayerPosition(Camera.UpVector * (-Speed * (float)GlobalValues.DeltaTime));

                }

                Camera.Update(Position + (0, CameraOffsetY, 0));

            }

        }

        public void GetBlockLooking(int range)
        {

            // DDA.Trace(ChunkLoader.ChunkDictionary, Camera.Position, Camera.ForwardVector, range);

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
