using OpenTK.Mathematics;
using System.Collections.Generic;

using Blockgame_OpenTK.ChunkUtil;
using System;
using Blockgame_OpenTK.BlockUtil;
using System.Runtime.CompilerServices;

namespace Blockgame_OpenTK.Util
{
    internal class DDA
    {

        public static Vector3 FinalPosition = Vector3.Zero;

        public static Vector3 HitLocal = Vector3.Zero;
        public static Vector3 HitGlobal = Vector3.Zero;
        public static Vector3 SmoothHit = Vector3.Zero;
        public static Vector3 SmoothPosition = Vector3.Zero;
        public static Vector3 ActualBlockPositionLocal = Vector3.Zero;
        public static Vector3 AChunkPosition = Vector3.Zero;

        public static Vector3 HitBlockPositionLocalToChunk = Vector3.Zero;
        public static Vector3 HitBlockPositionGlobal = Vector3.Zero;
        public static Vector3 HitChunkPosition = Vector3.Zero;



        public static Vector3i ChunkAtHit = Vector3i.Zero;
        public static Vector3i PositionAtHit = Vector3i.Zero;
        public static Vector3i PreviousPositionAtHit = Vector3i.Zero;
        public static Vector3i GlobalBlockPosition = Vector3i.Zero;

        public static Vector3i FaceHit = Vector3i.Zero;

        public static List<Vector3i> hitpositions;
        public static bool hit = false;
        public static void TraceChunks(Dictionary<Vector3i, NewChunk> chunkDictionary, Vector3 position, Vector3 direction, int maxSteps)
        {

            FaceHit = Vector3i.Zero;
            // RoundedPosition = (Vector3i) ChunkUtils.PositionToBlockGlobal(position);
            ChunkAtHit = Vector3i.Zero;
            PositionAtHit = Vector3i.Zero;

            // Vector3i CameraPositionRounded = (Vector3i)ChunkUtils.PositionToBlockGlobal(position);
            GlobalBlockPosition = (Vector3i) ChunkUtils.PositionToBlockGlobal(position);
            PreviousPositionAtHit = GlobalBlockPosition;
            Vector3i CameraPositionToBlockPositionGlobal = (Vector3i)ChunkUtils.PositionToBlockGlobal(position);

            Vector3i Step = Vector3i.Zero;
            Vector3 SideDistance = Vector3.Zero;
            Vector3 NormalizedDirection = Vector3.Normalize(direction);
            Vector3 DeltaDistance = (Math.Abs(1 / NormalizedDirection.X), Math.Abs(1 / NormalizedDirection.Y), Math.Abs(1 / NormalizedDirection.Z));
            float Distance = 0;

            if (NormalizedDirection.X < 0)
            {

                Step.X = -1;
                SideDistance.X = (position.X - GlobalBlockPosition.X) * DeltaDistance.X;

            }
            if (NormalizedDirection.X >= 0)
            {

                Step.X = 1;
                SideDistance.X = (GlobalBlockPosition.X + 1f - position.X) * DeltaDistance.X;

            }
            if (NormalizedDirection.Y < 0)
            {

                Step.Y = -1;
                SideDistance.Y = (position.Y - GlobalBlockPosition.Y) * DeltaDistance.Y;

            }
            if (NormalizedDirection.Y >= 0)
            {

                Step.Y = 1;
                SideDistance.Y = (GlobalBlockPosition.Y + 1f - position.Y) * DeltaDistance.Y;

            }
            if (NormalizedDirection.Z < 0)
            {

                Step.Z = -1;
                SideDistance.Z = (position.Z - GlobalBlockPosition.Z) * DeltaDistance.Z;

            }
            if (NormalizedDirection.Z >= 0)
            {

                Step.Z = 1;
                SideDistance.Z = (GlobalBlockPosition.Z + 1f - position.Z) * DeltaDistance.Z;

            }

            HitLocal = Vector3.Zero;
            HitGlobal = Vector3.Zero;
            SmoothHit = Vector3.Zero;
            SmoothPosition = Vector3.Zero;

            // hitpositions = new List<Vector3i>();

            // Console.WriteLine("start");
            hit = false;

            if (ChunkLoader.ContainsGeneratedChunk(ChunkUtils.PositionToChunk(GlobalBlockPosition)))
            {

                while (Maths.ChebyshevDistance3D(position, GlobalBlockPosition) < maxSteps && !hit)
                {

                    if (ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(GlobalBlockPosition)).GetBlock(ChunkUtils.PositionToBlockLocal(GlobalBlockPosition)) != Blocks.AirBlock)
                    {

                        // ChunkAtHit = (Vector3i)ChunkUtils.PositionToChunk(RoundedPosition);
                        // PositionAtHit = RoundedPosition;
                        ChunkAtHit = (Vector3i)ChunkUtils.PositionToChunk(GlobalBlockPosition);
                        hit = true;
                        PositionAtHit = GlobalBlockPosition;
                        SmoothPosition = position + NormalizedDirection * Distance;

                    }
                    else
                    {
                        PreviousPositionAtHit = GlobalBlockPosition;
                        if (SideDistance.X < SideDistance.Y)
                        {
                            // PreviousPositionAtHit = GlobalBlockPosition;
                            if (SideDistance.X < SideDistance.Z)
                            {

                                Distance = SideDistance.X;
                                SideDistance.X += DeltaDistance.X;
                                //PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.X += Step.X;

                            }
                            else
                            {

                                Distance = SideDistance.Z;
                                SideDistance.Z += DeltaDistance.Z;
                                //PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.Z += Step.Z;

                            }
                            //PreviousPositionAtHit = GlobalBlockPosition;

                        }
                        else
                        {

                            //PreviousPositionAtHit = GlobalBlockPosition;
                            if (SideDistance.Y < SideDistance.Z)
                            {

                                Distance = SideDistance.Y;
                                SideDistance.Y += DeltaDistance.Y;
                                //PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.Y += Step.Y;

                            }
                            else
                            {

                                Distance = SideDistance.Z;
                                SideDistance.Z += DeltaDistance.Z;
                                //PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.Z += Step.Z;

                            }
                            //PreviousPositionAtHit = GlobalBlockPosition;

                        }
                        // PreviousPositionAtHit = GlobalBlockPosition;

                    }

                }

            }


            // Console.WriteLine("end");

        }

        public static void TraceChunksWhile(Dictionary<Vector3i, NewChunk> chunkDictionary, Vector3 position, Vector3 direction, int maxSteps)
        {

            FaceHit = Vector3i.Zero;
            // RoundedPosition = (Vector3i) ChunkUtils.PositionToBlockGlobal(position);
            ChunkAtHit = Vector3i.Zero;
            PositionAtHit = Vector3i.Zero;

            // Vector3i CameraPositionRounded = (Vector3i)ChunkUtils.PositionToBlockGlobal(position);
            GlobalBlockPosition = (Vector3i)ChunkUtils.PositionToBlockGlobal(position);
            PreviousPositionAtHit = GlobalBlockPosition;
            Vector3i CameraPositionToBlockPositionGlobal = (Vector3i)ChunkUtils.PositionToBlockGlobal(position);

            Vector3i Step = Vector3i.Zero;
            Vector3 SideDistance = Vector3.Zero;
            Vector3 NormalizedDirection = Vector3.Normalize(direction);
            Vector3 DeltaDistance = (Math.Abs(1 / NormalizedDirection.X), Math.Abs(1 / NormalizedDirection.Y), Math.Abs(1 / NormalizedDirection.Z));
            float Distance = 0;

            if (NormalizedDirection.X < 0)
            {

                Step.X = -1;
                SideDistance.X = (position.X - GlobalBlockPosition.X) * DeltaDistance.X;

            }
            if (NormalizedDirection.X >= 0)
            {

                Step.X = 1;
                SideDistance.X = (GlobalBlockPosition.X + 1f - position.X) * DeltaDistance.X;

            }
            if (NormalizedDirection.Y < 0)
            {

                Step.Y = -1;
                SideDistance.Y = (position.Y - GlobalBlockPosition.Y) * DeltaDistance.Y;

            }
            if (NormalizedDirection.Y >= 0)
            {

                Step.Y = 1;
                SideDistance.Y = (GlobalBlockPosition.Y + 1f - position.Y) * DeltaDistance.Y;

            }
            if (NormalizedDirection.Z < 0)
            {

                Step.Z = -1;
                SideDistance.Z = (position.Z - GlobalBlockPosition.Z) * DeltaDistance.Z;

            }
            if (NormalizedDirection.Z >= 0)
            {

                Step.Z = 1;
                SideDistance.Z = (GlobalBlockPosition.Z + 1f - position.Z) * DeltaDistance.Z;

            }

            HitLocal = Vector3.Zero;
            HitGlobal = Vector3.Zero;
            SmoothHit = Vector3.Zero;
            SmoothPosition = Vector3.Zero;

            // hitpositions = new List<Vector3i>();

            // Console.WriteLine("start");

            int manhattanDistance = Maths.ManhattanDistance3D(position, GlobalBlockPosition);
            
            bool hit = false;

            // Console.WriteLine(manhattanDistance);

            if (ChunkLoader.ContainsGeneratedChunk(ChunkUtils.PositionToChunk(GlobalBlockPosition)))
            {

                while (position.X < maxSteps && !hit)
                {

                    if (ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(GlobalBlockPosition)).GetBlock(ChunkUtils.PositionToBlockLocal(GlobalBlockPosition)) != Blocks.AirBlock)
                    {

                        // ChunkAtHit = (Vector3i)ChunkUtils.PositionToChunk(RoundedPosition);
                        // PositionAtHit = RoundedPosition;
                        ChunkAtHit = (Vector3i)ChunkUtils.PositionToChunk(GlobalBlockPosition);
                        PositionAtHit = GlobalBlockPosition;
                        hit = true;
                        SmoothPosition = position + NormalizedDirection * Distance;

                    }
                    else
                    {
                        PreviousPositionAtHit = GlobalBlockPosition;
                        if (SideDistance.X < SideDistance.Y)
                        {
                            // PreviousPositionAtHit = GlobalBlockPosition;
                            if (SideDistance.X < SideDistance.Z)
                            {

                                Distance = SideDistance.X;
                                SideDistance.X += DeltaDistance.X;
                                //PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.X += Step.X;

                            }
                            else
                            {

                                Distance = SideDistance.Z;
                                SideDistance.Z += DeltaDistance.Z;
                                //PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.Z += Step.Z;

                            }
                            //PreviousPositionAtHit = GlobalBlockPosition;

                        }
                        else
                        {

                            //PreviousPositionAtHit = GlobalBlockPosition;
                            if (SideDistance.Y < SideDistance.Z)
                            {

                                Distance = SideDistance.Y;
                                SideDistance.Y += DeltaDistance.Y;
                                //PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.Y += Step.Y;

                            }
                            else
                            {

                                Distance = SideDistance.Z;
                                SideDistance.Z += DeltaDistance.Z;
                                //PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.Z += Step.Z;

                            }
                            //PreviousPositionAtHit = GlobalBlockPosition;

                        }
                        // PreviousPositionAtHit = GlobalBlockPosition;

                    }

                }

            }

            


            // Console.WriteLine("end");

        }

        public static void Trace(Dictionary<Vector3, Chunk> chunkDictionary, Vector3 position, Vector3 direction, int maxSteps)
        {

            Vector3i Step = Vector3i.Zero;
            Vector3 SideDistance = Vector3.Zero;
            Vector3i BlockPositionGlobal = (Vector3i)ChunkUtils.PositionToBlockGlobal(position);
            Vector3i BlockPositionLocal = (Vector3i)ChunkUtils.PositionToBlockLocal(position);
            Vector3i ChunkPosition = (Vector3i)ChunkUtils.PositionToChunk(position);
            Vector3 ChunkPositionBounds = ChunkUtils.PositionToChunkBounds(position);
            // Console.WriteLine(ChunkPositionBounds);
            Vector3 DeltaDistance = (Math.Abs(1 / direction.X), Math.Abs(1 / direction.Y), Math.Abs(1 / direction.Z));
            float Distance = 0;

            if (direction.X < 0)
            {

                Step.X = -1;
                SideDistance.X = (ChunkPositionBounds.X - BlockPositionLocal.X) * DeltaDistance.X;

            }
            if (direction.X >= 0)
            {

                Step.X = 1;
                SideDistance.X = (BlockPositionLocal.X + 1f - ChunkPositionBounds.X) * DeltaDistance.X;

            }
            if (direction.Y < 0)
            {

                Step.Y = -1;
                SideDistance.Y = (ChunkPositionBounds.Y - BlockPositionLocal.Y) * DeltaDistance.Y;

            }
            if (direction.Y >= 0)
            {

                Step.Y = 1;
                SideDistance.Y = (BlockPositionLocal.Y + 1f - ChunkPositionBounds.Y) * DeltaDistance.Y;

            }
            if (direction.Z < 0)
            {

                Step.Z = -1;
                SideDistance.Z = (ChunkPositionBounds.Z - BlockPositionLocal.Z) * DeltaDistance.Z;

            }
            if (direction.Z >= 0)
            {

                Step.Z = 1;
                SideDistance.Z = (BlockPositionLocal.Z + 1f - ChunkPositionBounds.Z) * DeltaDistance.Z;

            }

            HitLocal = Vector3.Zero;
            HitGlobal = Vector3.Zero;
            SmoothHit = Vector3.Zero;
            SmoothPosition = Vector3.Zero;

            // Console.WriteLine("{0}, {1}, {2}", BlockPositionLocal, ChunkPosition, ChunkPositionBounds);

            for (int i = 0; i < maxSteps; i++)
            {

                /* if (ChunkLoader.GetChunk(ChunkPosition).GetBlock(BlockPositionLocal) != Blocks.Air)
                {

                    HitLocal = BlockPositionLocal;
                    HitGlobal = BlockPositionLocal + (ChunkPosition * Globals.ChunkSize);
                    SmoothHit = SideDistance;
                    SmoothPosition = position + direction * Distance;

                } else
                {

                    if (SideDistance.X < SideDistance.Y)
                    {

                        if (SideDistance.X < SideDistance.Z)
                        {

                            Distance = SideDistance.X;
                            SideDistance.X += DeltaDistance.X;
                            BlockPositionLocal.X += Step.X;

                        }
                        else
                        {

                            Distance = SideDistance.Z;
                            SideDistance.Z += DeltaDistance.Z;
                            BlockPositionLocal.Z += Step.Z;

                        }

                    }
                    else
                    {

                        if (SideDistance.Y < SideDistance.Z)
                        {

                            Distance = SideDistance.Y;
                            SideDistance.Y += DeltaDistance.Y;
                            BlockPositionLocal.Y += Step.Y;

                        }
                        else
                        {

                            Distance = SideDistance.Z;
                            SideDistance.Z += DeltaDistance.Z;
                            BlockPositionLocal.Z += Step.Z;

                        }

                    }

                } */

                // Console.WriteLine(ChunkUtils.PositionToBlockLocal(BlockPositionLocal));

            }

        }

    }
}
