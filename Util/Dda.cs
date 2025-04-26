using Game.BlockUtil;
using Game.Core.Chunks;
using Game.Core.Worlds;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Game.Util
{
    public class Dda
    {

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
        public static bool DidHit = false;

        /*
        Implemented based on a few links
        https://www.researchgate.net/publication/233899848_Efficient_implementation_of_the_3D-DDA_ray_traversal_algorithm_on_GPU_and_its_application_in_radiation_dose_calculation
        https://www.shadertoy.com/view/4dX3zl
        */
        public static void TraceChunks(ConcurrentDictionary<Vector2i, ChunkColumn> chunkDictionary, Vector3 position, Vector3 direction, int maxSteps)
        {

            FaceHit = Vector3i.Zero;
            ChunkAtHit = Vector3i.Zero;
            PositionAtHit = Vector3i.Zero;

            GlobalBlockPosition = ChunkUtils.PositionToBlockGlobal(position);
            PreviousPositionAtHit = GlobalBlockPosition;

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
            DidHit = false;

            while (Maths.ChebyshevDistance3D(position, GlobalBlockPosition) < maxSteps && !DidHit)
            {

                Vector3i chunkPosition = ChunkUtils.PositionToChunk(GlobalBlockPosition);

                if (chunkPosition.Y < WorldGenerator.WorldGenerationHeight && chunkPosition.Y >= 0 && chunkDictionary.ContainsKey(chunkPosition.Xz) && !chunkDictionary[chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates)
                {
                    
                    if (chunkDictionary[ChunkUtils.PositionToChunk(GlobalBlockPosition).Xz].Chunks[ChunkUtils.PositionToChunk(GlobalBlockPosition).Y].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(GlobalBlockPosition))] != 0)
                    {

                        ChunkAtHit = ChunkUtils.PositionToChunk(GlobalBlockPosition);
                        DidHit = true;
                        PositionAtHit = GlobalBlockPosition;
                        SmoothPosition = position + NormalizedDirection * Distance;

                    } else
                    {

                        PreviousPositionAtHit = GlobalBlockPosition;
                        if (SideDistance.X < SideDistance.Y)
                        {

                            if (SideDistance.X < SideDistance.Z)
                            {

                                Distance = SideDistance.X;
                                SideDistance.X += DeltaDistance.X;
                                GlobalBlockPosition.X += Step.X;

                            }
                            else
                            {

                                Distance = SideDistance.Z;
                                SideDistance.Z += DeltaDistance.Z;
                                GlobalBlockPosition.Z += Step.Z;

                            }

                        }
                        else
                        {

                            PreviousPositionAtHit = GlobalBlockPosition;
                            if (SideDistance.Y < SideDistance.Z)
                            {

                                Distance = SideDistance.Y;
                                SideDistance.Y += DeltaDistance.Y;
                                GlobalBlockPosition.Y += Step.Y;

                            }
                            else
                            {

                                Distance = SideDistance.Z;
                                SideDistance.Z += DeltaDistance.Z;
                                GlobalBlockPosition.Z += Step.Z;

                            }

                        }

                    }

                } else
                {

                    PreviousPositionAtHit = GlobalBlockPosition;
                    if (SideDistance.X < SideDistance.Y)
                    {
                        
                        if (SideDistance.X < SideDistance.Z)
                        {

                            Distance = SideDistance.X;
                            SideDistance.X += DeltaDistance.X;
                            GlobalBlockPosition.X += Step.X;

                        }
                        else
                        {

                            Distance = SideDistance.Z;
                            SideDistance.Z += DeltaDistance.Z;
                            GlobalBlockPosition.Z += Step.Z;

                        }

                    }
                    else
                    {

                        PreviousPositionAtHit = GlobalBlockPosition;
                        if (SideDistance.Y < SideDistance.Z)
                        {

                            Distance = SideDistance.Y;
                            SideDistance.Y += DeltaDistance.Y;
                            GlobalBlockPosition.Y += Step.Y;

                        }
                        else
                        {

                            Distance = SideDistance.Z;
                            SideDistance.Z += DeltaDistance.Z;
                            GlobalBlockPosition.Z += Step.Z;

                        }

                    }

                }


            }

        }

    }

}
