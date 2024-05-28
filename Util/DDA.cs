using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

using Blockgame_OpenTK.ChunkUtil;
using Blockgame_OpenTK.BlockUtil;

namespace Blockgame_OpenTK.Util
{
    internal class DDA
    {

        public static Vector3 HitPoint = Vector3.Zero;
        public static List<Vector3> pos = new List<Vector3>();

        public static void Trace(Chunk chunk, Vector3 position, Vector3 direction, float maxSteps)
        {

            // Vector3 LocalPosition = ChunkUtils.PositionToChunkBounds(position);
            Vector3 PositionInteger = ChunkUtils.PositionToBlockGlobal(position);
            Vector3 LocalPosition = ChunkUtils.PositionToBlockLocalToChunk(PositionInteger);
            Vector3 DeltaDistance = (Math.Abs(1 / direction.X), Math.Abs(1 / direction.Y), Math.Abs(1 / direction.Z));
            Vector3 Step = Vector3.Zero;
            Vector3 SideDistance = Vector3.Zero;
            if (direction.X < 0)
            {

                Step.X = -1;
                SideDistance.X = (position.X - PositionInteger.X) * DeltaDistance.X;

            } else
            {

                Step.X = 1;
                SideDistance.X = (PositionInteger.X + 1 - position.X) * DeltaDistance.X;

            }
            if (direction.Y < 0)
            {

                Step.Y = -1;
                SideDistance.Y = (position.Y - PositionInteger.Y) * DeltaDistance.Y;

            }
            else
            {

                Step.Y = 1;
                SideDistance.Y = (PositionInteger.Y + 1 - position.Y) * DeltaDistance.Y;

            }
            if (direction.Z < 0)
            {

                Step.Z = -1;
                SideDistance.Z = (position.Z - PositionInteger.Z) * DeltaDistance.Z;

            }
            else
            {

                Step.Z = 1;
                SideDistance.Z = (PositionInteger.Z + 1 - position.Z) * DeltaDistance.Z;

            }

            Console.WriteLine(Step);

            // Console.WriteLine(PositionInteger + ", " + LocalPosition);

            float d = 0;

            List<Vector3> positions = new List<Vector3>();

            for (int i = 0; i < maxSteps; i++)
            {

                if (chunk.GetBlock((int)LocalPosition.X, (int)LocalPosition.Y, (int)LocalPosition.Z) != Blocks.Air) { continue; }

                if (SideDistance.X < SideDistance.Y)
                {

                    if (SideDistance.X < SideDistance.Z)
                    {

                        d = SideDistance.X;
                        SideDistance.X += DeltaDistance.X;
                        PositionInteger.X += Step.X;
                        positions.Add(PositionInteger);

                    }
                    else
                    {
                        d = SideDistance.Z;
                        SideDistance.Z += DeltaDistance.Z;
                        PositionInteger.Z += Step.Z;
                        positions.Add(PositionInteger);
                    }

                }
                else
                {

                    if (SideDistance.Y < SideDistance.Z)
                    {
                        d = SideDistance.Y;
                        SideDistance.Y += DeltaDistance.Y;
                        PositionInteger.Y += Step.Y;
                        positions.Add(PositionInteger);
                    } else
                    {

                        d = SideDistance.Z;
                        SideDistance.Z += DeltaDistance.Z;
                        PositionInteger.Z += Step.Z;
                        positions.Add(PositionInteger);
                    }

                }

                // Console.WriteLine(Blocks.GetIDFromBlock(ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(position)).GetBlock((int)LocalPosition.X, (int)LocalPosition.Y, (int)LocalPosition.Z)));

            }

            HitPoint = PositionInteger;
            pos = positions;
        }

    }
}
