using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using Blockgame_OpenTK.Util;
using System;
using Blockgame_OpenTK.ChunkUtil;
using Blockgame_OpenTK.BlockUtil;
using OpenTK.Windowing.Common;
using Blockgame_OpenTK.Core.World;

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

        public void Update(World world)
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

                        Dda.TraceChunks(world.WorldChunks, Camera.Position, Camera.ForwardVector, Globals.PlayerRange);
                        if (Dda.hit)
                        {

                            Vector3i HitPositionLocal = (Vector3i)ChunkUtils.PositionToBlockLocal(Dda.PositionAtHit);

                            world.WorldChunks[Dda.ChunkAtHit].SetBlock(HitPositionLocal, Blocks.AirBlock);
                            WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit);
                            WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit + Vector3i.UnitZ);
                            WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit - Vector3i.UnitZ);
                            WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit + Vector3i.UnitX);
                            WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit - Vector3i.UnitX);
                            WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit + Vector3i.UnitY);
                            WorldGenerator.ChunkAlterUpdateQueue.Enqueue(Dda.ChunkAtHit - Vector3i.UnitY);


                            // if (ChunkLoader.ContainsGeneratedChunk(DDA.ChunkAtHit))
                            {

                                // ChunkLoader.GetChunk(DDA.ChunkAtHit).SetBlockRewrite(Blocks.AirBlock, HitPositionLocal);
                                // ChunkLoader.GetChunk(DDA.ChunkAtHit).BlockData[HitPositionLocal.X, HitPositionLocal.Y, HitPositionLocal.Z] = (ushort) Globals.Register.GetIDFromBlock(Blocks.AirBlock);
                                // ChunkBuilder.CallOpenGL(ChunkLoader.GetChunk(DDA.ChunkAtHit));

                                // ChunkLoader.GetChunk(DDA.ChunkAtHit).SetBlock(HitPositionLocal, Blocks.AirBlock);
                                // ChunkBuilder.Remesh(ChunkLoader.GetChunk(DDA.ChunkAtHit), ChunkLoader.GetChunkNeighbors(ChunkLoader.GetChunk(DDA.ChunkAtHit)));
                                // ChunkLoader.RemeshQueue.Add(DDA.ChunkAtHit);
                                // ChunkLoader.RemeshQueue.Add(ChunkUtils.PositionToChunk(DDA.PreviousPositionAtHit) + Vector3i.UnitX);
                                // ChunkLoader.RemeshQueue.Add(ChunkUtils.PositionToChunk(DDA.PreviousPositionAtHit) - Vector3i.UnitX);
                                // ChunkLoader.RemeshQueue.Add(ChunkUtils.PositionToChunk(DDA.PreviousPositionAtHit) + Vector3i.UnitY);
                                // ChunkLoader.RemeshQueue.Add(ChunkUtils.PositionToChunk(DDA.PreviousPositionAtHit) - Vector3i.UnitY);
                                // ChunkLoader.RemeshQueue.Add(ChunkUtils.PositionToChunk(DDA.PreviousPositionAtHit) + Vector3i.UnitZ);
                                // ChunkLoader.RemeshQueue.Add(ChunkUtils.PositionToChunk(DDA.PreviousPositionAtHit) - Vector3i.UnitZ);

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

                        Dda.TraceChunks(world.WorldChunks, Camera.Position, Camera.ForwardVector, Globals.PlayerRange);
                        
                        if (Dda.hit)
                        {

                            Vector3i HitPositionLocal = (Vector3i)ChunkUtils.PositionToBlockLocal(Dda.PreviousPositionAtHit);
                            Console.WriteLine("hitpos: {0}, prevpos: {1}", Dda.PositionAtHit, Dda.PreviousPositionAtHit);

                            if (Dda.PreviousPositionAtHit != ChunkUtils.PositionToBlockGlobal(Camera.Position))
                            {

                                Vector3i previousPositionChunkHit = ChunkUtils.PositionToChunk(Dda.PreviousPositionAtHit);

                                world.WorldChunks[previousPositionChunkHit].SetBlock(HitPositionLocal, Blocks.StoneBlock);
                                WorldGenerator.ChunkAlterUpdateQueue.Enqueue(previousPositionChunkHit);
                                WorldGenerator.ChunkAlterUpdateQueue.Enqueue(previousPositionChunkHit + Vector3i.UnitZ);
                                WorldGenerator.ChunkAlterUpdateQueue.Enqueue(previousPositionChunkHit - Vector3i.UnitZ);
                                WorldGenerator.ChunkAlterUpdateQueue.Enqueue(previousPositionChunkHit + Vector3i.UnitX);
                                WorldGenerator.ChunkAlterUpdateQueue.Enqueue(previousPositionChunkHit - Vector3i.UnitX);
                                WorldGenerator.ChunkAlterUpdateQueue.Enqueue(previousPositionChunkHit + Vector3i.UnitY);
                                WorldGenerator.ChunkAlterUpdateQueue.Enqueue(previousPositionChunkHit - Vector3i.UnitY);

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
