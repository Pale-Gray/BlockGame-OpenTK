using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using Blockgame_OpenTK.Util;
using System;
using Blockgame_OpenTK.ChunkUtil;
using Blockgame_OpenTK.BlockUtil;
using OpenTK.Windowing.Common;

namespace Blockgame_OpenTK.PlayerUtil
{
    internal class Player
    {

        public Vector3 Position = Vector3.Zero;
        public Camera Camera = new Camera(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, CameraType.Perspective, 90.0f);
        float CameraOffsetY = 0.0f;
        float WalkSpeed = 15.0f;
        float RunSpeed = 50.0f;

        float PressDelay = 1.0f;
        float PlaceDelay = 1.0f;
        float RemoveDelay = 1.0f;

        public Player() { }

        public void Update()
        {
            float Speed = 0;

            if (Globals.CursorState == CursorState.Grabbed)
            {

                if (Globals.Keyboard.IsKeyDown(Keys.LeftShift))
                {

                    Speed = RunSpeed;

                }
                else
                {

                    Speed = WalkSpeed;

                }

                if (Globals.Mouse.IsButtonDown(MouseButton.Left))
                {

                    RemoveDelay += (float)Globals.DeltaTime;

                    if (RemoveDelay >= 0.25f)
                    {

                        DDA.TraceChunks(ChunkLoader.Chunks, Camera.Position, Camera.ForwardVector, Globals.PlayerRange);
                        if (DDA.hit)
                        {

                            Vector3i HitPositionLocal = (Vector3i)ChunkUtils.PositionToBlockLocal(DDA.PositionAtHit);

                            if (ChunkLoader.ContainsGeneratedChunk(DDA.ChunkAtHit))
                            {

                                // ChunkLoader.GetChunk(DDA.ChunkAtHit).SetBlockRewrite(Blocks.AirBlock, HitPositionLocal);
                                // ChunkLoader.GetChunk(DDA.ChunkAtHit).BlockData[HitPositionLocal.X, HitPositionLocal.Y, HitPositionLocal.Z] = (ushort) Globals.Register.GetIDFromBlock(Blocks.AirBlock);
                                // ChunkBuilder.CallOpenGL(ChunkLoader.GetChunk(DDA.ChunkAtHit));

                                ChunkLoader.GetChunk(DDA.ChunkAtHit).SetBlock(HitPositionLocal, Blocks.AirBlock);
                                // ChunkBuilder.Remesh(ChunkLoader.GetChunk(DDA.ChunkAtHit), ChunkLoader.GetChunkNeighbors(ChunkLoader.GetChunk(DDA.ChunkAtHit)));
                                ChunkLoader.RemeshQueue.Add(DDA.ChunkAtHit);

                                if (HitPositionLocal.X == 0) { ChunkLoader.RemeshQueue.Add(DDA.ChunkAtHit - (1,0,0)); }
                                if (HitPositionLocal.X == Globals.ChunkSize - 1) { ChunkLoader.RemeshQueue.Add(DDA.ChunkAtHit + (1,0,0)); }
                                if (HitPositionLocal.Y == 0) { ChunkLoader.RemeshQueue.Add(DDA.ChunkAtHit - (0,1,0)); }
                                if (HitPositionLocal.Y == Globals.ChunkSize - 1) { ChunkLoader.RemeshQueue.Add(DDA.ChunkAtHit + (0,1,0)); }
                                if (HitPositionLocal.Z == 0) { ChunkLoader.RemeshQueue.Add(DDA.ChunkAtHit + (0,0,1)); }
                                if (HitPositionLocal.Z == Globals.ChunkSize - 1) { ChunkLoader.RemeshQueue.Add(DDA.ChunkAtHit - (0,0,1)); }

                            }

                            RemoveDelay = 0;

                        }
                        // ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(DDA.HitChunkPosition)).SetBlockRewrite(Blocks.Air, (Vector3i) DDA.HitBlockPositionLocalToChunk);
                        // ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(DDA.HitChunkPosition)).SetBlock((Vector3i)DDA.HitBlockPositionLocalToChunk, Blocks.AirBlock);

                    }

                } else { RemoveDelay = 1; }

                if (Globals.Mouse.IsButtonDown(MouseButton.Right))
                {

                    PlaceDelay += (float)Globals.DeltaTime;

                    if (PlaceDelay >= 0.25f)
                    {

                        DDA.TraceChunks(ChunkLoader.Chunks, Camera.Position, Camera.ForwardVector, Globals.PlayerRange);
                        
                        if (DDA.hit)
                        {

                            Vector3i HitPositionLocal = (Vector3i)ChunkUtils.PositionToBlockLocal(DDA.PositionAtHit);
                            Console.WriteLine("hitpos: {0}, prevpos: {1}", DDA.PositionAtHit, DDA.PreviousPositionAtHit);

                            //Vector3i BuildCoordinate = HitPositionLocal + BuildOffset;

                            if (ChunkLoader.ContainsChunk(ChunkUtils.PositionToChunk(DDA.PreviousPositionAtHit)))
                            {

                                if (ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(DDA.PositionAtHit)).GetBlock((Vector3i)ChunkUtils.PositionToBlockLocal(DDA.PositionAtHit)) != Blocks.AirBlock)
                                {

                                    // ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(DDA.PreviousPositionAtHit)).SetBlockRewrite(Blocks.GrassBlock, (Vector3i)ChunkUtils.PositionToBlockLocal(DDA.PreviousPositionAtHit));
                                    Vector3i blockLocal = ChunkUtils.PositionToBlockLocal(DDA.PreviousPositionAtHit);
                                    ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(DDA.PreviousPositionAtHit)).BlockData[blockLocal.X, blockLocal.Y, blockLocal.Z] = (ushort)Globals.Register.GetIDFromBlock(Blocks.StoneBlock);
                                    // ChunkBuilder.CallOpenGL(ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(DDA.PreviousPositionAtHit)));
                                    // ChunkBuilder.Remesh(ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(DDA.PreviousPositionAtHit)), ChunkLoader.GetChunkNeighbors(ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(DDA.PreviousPositionAtHit))));
                                    ChunkLoader.RemeshQueue.Add(ChunkUtils.PositionToChunk(DDA.PreviousPositionAtHit));

                                }

                            }

                            PlaceDelay = 0;

                        }

                    }

                } else {  PlaceDelay = 1; }

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

                Camera.Update(Position + (0, CameraOffsetY, 0));

            }

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
