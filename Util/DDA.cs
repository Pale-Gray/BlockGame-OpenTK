using OpenTK.Mathematics;
using System.Collections.Generic;

using Blockgame_OpenTK.ChunkUtil;
using System;
using Blockgame_OpenTK.BlockUtil;

namespace Blockgame_OpenTK.Util
{
    internal class DDA
    {

        public static Vector3 FinalPosition = Vector3.Zero;

        public static Vector3 HitLocal = Vector3.Zero;
        public static Vector3 HitGlobal = Vector3.Zero;
        public static Vector3 SmoothHit = Vector3.Zero;
        public static Vector3 SmoothPosition = Vector3.Zero;

        public static void Trace(Dictionary<Vector3, Chunk> chunkDictionary, Vector3 position, Vector3 direction, int maxSteps)
        {

            Vector3i Step = Vector3i.Zero;
            Vector3 SideDistance = Vector3.Zero;
            Vector3i BlockPositionGlobal = (Vector3i)ChunkUtils.PositionToBlockGlobal(position);
            Vector3i BlockPositionLocal = (Vector3i)ChunkUtils.PositionToBlockLocal(position);
            Vector3i ChunkPosition = (Vector3i)ChunkUtils.PositionToChunk(position);
            Vector3 ChunkPositionBounds = ChunkUtils.PositionToChunkBounds(position);
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

            // Console.WriteLine("Step: {0}, BPG: {1}, BPL: {2}, CHP: {3}, CHPB: {4}", Step, BlockPositionGlobal, BlockPositionLocal, ChunkPosition, ChunkPositionBounds);

            // Console.WriteLine("BP: {0}, BPL: {1}", BlockPositionLocal, ChunkUtils.PositionToBlockLocal(BlockPositionLocal));

            Console.WriteLine("{0}, {1}", BlockPositionLocal, ChunkPosition);

            HitLocal = Vector3.Zero;
            HitGlobal = Vector3.Zero;
            SmoothHit = Vector3.Zero;

            for (int i = 0; i < maxSteps; i++)
            {

                if (ChunkLoader.GetChunk(ChunkPosition).GetBlock(BlockPositionLocal) != Blocks.Air)
                {

                    HitLocal = BlockPositionLocal;
                    HitGlobal = BlockPositionLocal + (Globals.ChunkSize * ChunkPosition);
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

                }

                // Console.WriteLine(ChunkUtils.PositionToBlockLocal(BlockPositionLocal));

            }

        }

    }
}
