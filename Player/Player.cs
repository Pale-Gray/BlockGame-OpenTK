using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using Blockgame_OpenTK.Util;
using System;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.BlockUtil;
using OpenTK.Windowing.Common;
using Blockgame_OpenTK.Core.Worlds;
using System.Collections.Generic;
using System.Globalization;
using OpenTK.Platform;
using System.Runtime.InteropServices;

namespace Blockgame_OpenTK.PlayerUtil
{
    public class Player
    {

        public Vector3 Position = Vector3.Zero;
        public Camera Camera = new Camera(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, CameraType.Perspective, 90.0f);
        float CameraOffsetY = 0.0f;
        float WalkSpeed = 6.0f;
        float RunSpeed = 10.0f;

        float PressDelay = 1.0f;
        float PlaceDelay = 1.0f;
        float RemoveDelay = 1.0f;

        float Acceleration = 0.0f;
        float GravitationalAcceleration = -25.0f;
        float Mass = 64.0f;
        Vector3 Velocity = Vector3.Zero;

        bool IsGrounded = false;

        public AxisAlignedBoundingBox PlayerBounds = new AxisAlignedBoundingBox((0, 0, 0), (0.7f, 1.9f, 0.7f), (0.0f, 0.0f, 0.0f));

        public Player() { }

        public void Update(PackedChunkWorld world)
        {
            float Speed = 0;

            if (GlobalValues.IsCursorLocked)
            {

                if (Translator.ResolveKeymap("Sprint"))
                {

                    Speed = RunSpeed;

                }
                else
                {

                    Speed = WalkSpeed;

                }

                if (Translator.ResolveKeymap("DestroyBlock") || Input.IsLeftTriggerDown())
                {

                    RemoveDelay += (float)GlobalValues.DeltaTime;

                    if (RemoveDelay >= 0.15f)
                    {

                        Dda.TraceChunks(world.WorldColumns, Camera.Position, Camera.ForwardVector, GlobalValues.PlayerRange);
                        if (Dda.hit)
                        {

                            Vector3i HitPositionLocal = (Vector3i)ChunkUtils.PositionToBlockLocal(Dda.PositionAtHit);
                            // new NewBlock().OnBlockDestroy(PackedWorldGenerator.CurrentWorld, Dda.PositionAtHit);
                            // ushort id = world.PackedWorldChunks[ChunkUtils.PositionToChunk(Dda.PositionAtHit)].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(Dda.PositionAtHit))];
                            ushort id = world.WorldColumns[ChunkUtils.PositionToChunk(Dda.PositionAtHit).Xz].Chunks[ChunkUtils.PositionToChunk(Dda.PositionAtHit).Y].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(Dda.PositionAtHit))];
                            GlobalValues.NewRegister.GetBlockFromId(id).OnBlockDestroy(world, Dda.PositionAtHit);
                            // world.WorldChunks[ChunkUtils.PositionToChunk(Dda.PositionAtHit)].GetBlock(HitPositionLocal).OnBlockDestroy(world, Dda.PositionAtHit);
                            
                            RemoveDelay = 0;

                        }

                    }

                }
                else { RemoveDelay = 1; }

                if (Translator.ResolveKeymap("PlaceBlock") || Input.IsRightTriggerDown() || Input.IsKeyDown(Key.RightShift))
                {

                    PlaceDelay += (float)GlobalValues.DeltaTime;

                    if (PlaceDelay >= 0.15f)
                    {

                        // Dda.TraceChunks(world.WorldChunks, Camera.Position, Camera.ForwardVector, GlobalValues.PlayerRange);

                        if (Dda.hit)
                        {

                            Vector3i HitPositionLocal = (Vector3i)ChunkUtils.PositionToBlockLocal(Dda.PreviousPositionAtHit);
                            // Console.WriteLine("hitpos: {0}, prevpos: {1}", Dda.PositionAtHit, Dda.PreviousPositionAtHit);

                            if (Dda.PreviousPositionAtHit != ChunkUtils.PositionToBlockGlobal(Camera.Position))
                            {

                                Vector3i previousPositionChunkHit = ChunkUtils.PositionToChunk(Dda.PreviousPositionAtHit);

                                GlobalValues.NewRegister.GetBlockFromId(GlobalValues.BlockSelectorID).OnBlockPlace(PackedWorldGenerator.CurrentWorld, Dda.PreviousPositionAtHit);
                                // GlobalValues.Register.GetBlockFromID(GlobalValues.BlockSelectorID).OnBlockPlace(world, Dda.PreviousPositionAtHit);

                            }

                            PlaceDelay = 0;

                        }

                    }

                }
                else { PlaceDelay = 1; }

                Vector3 positionAlter = Vector3.Zero;

                if (Input.JoystickLeftAxis != Vector2.Zero)
                {

                    float xValue = ((float) (Math.Cos(Maths.ToRadians(Camera.Yaw))) * Input.JoystickLeftAxis.Y) + ((float)(Math.Sin(Maths.ToRadians(Camera.Yaw))) * -Input.JoystickLeftAxis.X);
                    float zValue = ((float) (Math.Sin(Maths.ToRadians(Camera.Yaw))) * Input.JoystickLeftAxis.Y) + ((float)(Math.Cos(Maths.ToRadians(Camera.Yaw))) * Input.JoystickLeftAxis.X);

                    positionAlter += new Vector3(xValue, 0.0f, zValue) * Speed;

                }
                if (Translator.ResolveKeymap("Forwards"))
                {

                    float xValue = (float) Math.Cos(Maths.ToRadians(Camera.Yaw));
                    float zValue = (float) Math.Sin(Maths.ToRadians(Camera.Yaw));

                    positionAlter += new Vector3(xValue, 0.0f, zValue) * Speed;//  * (float)GlobalValues.DeltaTime;

                    // Velocity = new Vector3(Camera.ForwardVector.X, 0.0f, Camera.ForwardVector.Z) * Speed;// * (float)GlobalValues.DeltaTime;
                    // Velocity.X = positionAlter.X;
                    // Velocity.Z = positionAlter.Z;

                }
                if (Translator.ResolveKeymap("Left"))
                {

                    float xValue = (float)Math.Cos(Maths.ToRadians(Camera.Yaw));
                    float zValue = (float)Math.Sin(Maths.ToRadians(Camera.Yaw));

                    Vector3 vec = -Vector3.Normalize(Vector3.Cross((xValue, 0.0f, zValue), Camera.UpVector)) * Speed;// * (float)GlobalValues.DeltaTime);

                    positionAlter += (vec.X, 0.0f, vec.Z);

                    // Velocity.X = positionAlter.X;
                    // Velocity.Z = positionAlter.Z;

                }
                if (Translator.ResolveKeymap("Backwards"))
                {

                    float xValue = (float)Math.Cos(Maths.ToRadians(Camera.Yaw));
                    float zValue = (float)Math.Sin(Maths.ToRadians(Camera.Yaw));

                    Vector3 vec = -new Vector3(xValue, 0.0f, zValue) * Speed;// * (float)GlobalValues.DeltaTime;

                    positionAlter += (vec.X, 0.0f, vec.Z);

                    // Velocity.X = positionAlter.X;
                    // Velocity.Z = positionAlter.Z;

                }
                if (Translator.ResolveKeymap("Right"))
                {

                    float xValue = (float)Math.Cos(Maths.ToRadians(Camera.Yaw));
                    float zValue = (float)Math.Sin(Maths.ToRadians(Camera.Yaw));

                    Vector3 vec = Vector3.Normalize(Vector3.Cross((xValue, 0.0f, zValue), Camera.UpVector)) * Speed;

                    positionAlter += (vec.X, 0.0f, vec.Z);

                }

                if (positionAlter.Length > Speed)
                {

                    positionAlter.Normalize();
                    positionAlter *= Speed;

                }

                if (positionAlter.Length != 0)
                {

                    Velocity.X = positionAlter.X;
                    Velocity.Z = positionAlter.Z;

                }

                // if (Velocity.X != 0) Velocity.X = positionAlter.X;
                // if (Velocity.Z != 0) Velocity.Z = positionAlter.Z;

                if (Translator.ResolveKeymap("Up"))
                {

                    AddPlayerPosition(Camera.UpVector * (Speed * (float)GlobalValues.DeltaTime));
                    Velocity.Y = 0.0f;

                }

                if (Translator.ResolveKeymap("Down"))
                {

                    AddPlayerPosition(Camera.UpVector * (-Speed * (float)GlobalValues.DeltaTime));
                    Velocity.Y = 0.0f;

                }

                Vector3i min = ChunkUtils.PositionToBlockGlobal(Position);
                Vector3i max = ChunkUtils.PositionToBlockGlobal(min + PlayerBounds.Dimensions);

                PriorityQueue<AxisAlignedBoundingBox, float> boundPriorityQueue = new PriorityQueue<AxisAlignedBoundingBox, float>();

                Camera.Update(Position + (0.5f * PlayerBounds.Dimensions.X, CameraOffsetY, 0.5f * PlayerBounds.Dimensions.Z));

                Position += Velocity * (float)GlobalValues.DeltaTime;

                AxisAlignedBoundingBox playerBoundsOffsetted = PlayerBounds.GetOffsetBoundingBox(Position);

                for (int x = min.X - 1; x <= max.X + 1; x++)
                {

                    for (int y = min.Y - 1; y <= max.Y + 1; y++)
                    {

                        for (int z = min.Z - 1; z <= max.Z + 1; z++)
                        {

                            if (world.WorldColumns.ContainsKey(ChunkUtils.PositionToChunk((x,y,z)).Xz) && ColumnUtils.GetSolidBlock(world.WorldColumns[ChunkUtils.PositionToChunk((x,y,z)).Xz], (x,y,z)))
                            // world.PackedWorldChunks[ChunkUtils.PositionToChunk((x,y,z))].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal((x,y,z)))] != 0)
                            {

                                // Block sampledBlock = world.WorldChunks[ChunkUtils.PositionToChunk((x, y, z))].GetBlock(ChunkUtils.PositionToBlockLocal((x,y,z)));
                                AxisAlignedBoundingBox blockBoundingBox = new AxisAlignedBoundingBox((0, 0, 0), (1, 1, 1), (0, 0, 0)).GetOffsetBoundingBox((x, y, z));// sampledBlock.BoundingBox.GetOffsetBoundingBox((x, y, z));

                                if (playerBoundsOffsetted.CollideWith(blockBoundingBox))// && (sampledBlock.HasCollision ?? true))
                                {

                                    boundPriorityQueue.Enqueue(blockBoundingBox, Maths.Dist3D(playerBoundsOffsetted.Position, blockBoundingBox.Position));

                                }

                            }

                        }

                    }

                }

                float lastDynamicFriction = 0.0f;

                while (boundPriorityQueue.Count > 0)
                {

                    if (boundPriorityQueue.TryDequeue(out AxisAlignedBoundingBox bound, out float distance))
                    {

                        if (playerBoundsOffsetted.CollideWith(bound))
                        {

                            Vector3 depthVector = playerBoundsOffsetted.GetDepthVector(bound);

                            Vector3 normal = Vector3.Zero;
                            if (depthVector.X != 0) normal.X = 1 * float.Sign(depthVector.X);
                            if (depthVector.Y != 0) normal.Y = 1 * float.Sign(depthVector.Y);
                            if (depthVector.Z != 0) normal.Z = 1 * float.Sign(depthVector.Z);

                            if (depthVector.Y > 0)
                            {

                                IsGrounded = true;

                            } else { IsGrounded = false; }

                            Vector3 relativeVelocity = -Velocity;
                            float e = 0.0f;
                            float j = (-(1.0f + e) * Vector3.Dot(normal, relativeVelocity)) / (1 / Mass);
                            Vector3 impulseForce = (j * normal);

                            Vector3 tangent = relativeVelocity - Vector3.Dot(relativeVelocity, normal) * normal;
                            if (tangent.Length >= 1) tangent.Normalize();

                            float jt = -Vector3.Dot(relativeVelocity, tangent);

                            Vector3 frictionForce = Vector3.Zero;

                            if (Math.Abs(jt) <= j * bound.StaticFriction)
                            {

                                frictionForce = -jt * tangent;

                            } else
                            {

                                frictionForce = j * tangent * 0.8f;

                            }

                            Velocity -= (impulseForce) / Mass;

                            Velocity.X /= (1 + 0.8f);
                            Velocity.Z /= (1 + 0.8f);

                            Position += depthVector;

                            playerBoundsOffsetted = PlayerBounds.GetOffsetBoundingBox(Position);

                        }

                    }

                }

                if (Translator.ResolveKeymap("Jump") || Input.IsJoystickButtonDown(JoystickButton.A))
                {

                    if (IsGrounded)
                    {

                        Velocity.Y = 8.0f;
                        IsGrounded = false;

                    }

                }

                // Velocity.X /= (1.0f + lastDynamicFriction) * (float)GlobalValues.DeltaTime;
                // Velocity.Z /= (1.0f + lastDynamicFriction) * (float)GlobalValues.DeltaTime;

                Velocity.Y += GravitationalAcceleration * (float)GlobalValues.DeltaTime;

                // Position += positionAlter;

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
