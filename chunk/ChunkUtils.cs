using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK.ChunkUtil
{
    internal class ChunkUtils
    {

        static int Steps = 0;
        static int FB = 1;
        static int LR = 0;
        static Vector3 SpiralOrigin = Vector3.Zero;
        static Vector3 SpiralPosition = new Vector3(SpiralOrigin);
        
        public static Vector3 GetSpiralPosition()
        {

            return SpiralPosition;

        }

        public static bool FinishedSteps(int radius)
        {

            if (Steps >= ((int)Math.Pow(Maths.Dist(-radius, 0, radius, 0) + 1, 2)) - 1)
            {

                return true;

            } else
            {

                return false;

            }

        }
        public static void StepSpiral(int radius)
        {

            if (Steps >= ((int)Math.Pow(Maths.Dist(-radius, 0, radius, 0) + 1, 2)) - 1)
            {

                // return SpiralPosition;

            }
            else
            {

                if (SpiralPosition.Z >= radius)
                {

                    FB = 0;
                    LR = -1;

                }
                if (SpiralPosition.X <= -radius)
                {

                    LR = 0;
                    FB = -1;

                }
                if (SpiralPosition.Z <= -radius)
                {

                    FB = 0;
                    LR = 1;

                }
                if (SpiralPosition.X >= radius)
                {

                    LR = 0;
                    FB = 1;

                }

                // Vector3 Position = new Vector3(SpiralPosition);

                SpiralPosition += (LR, 0, FB);

                Steps++;

            }

        }

        public static Vector3[] LoopColumn(Vector3 position, int radius, int constraint)
        {

            List<Vector3> vectors = new List<Vector3>();

            foreach (Vector3 RingPosition in SingleRing(position, radius))
            {

                foreach (Vector3 Position in Column(RingPosition, constraint))
                {

                    vectors.Add(Position);

                }

            }

            return vectors.ToArray();

        }
        public static Vector3[] Cube(Vector3 position, int radius, int constraint)
        {

            List<Vector3> list = new List<Vector3>();

            for (int x = (int)position.X - radius; x <= (int)position.X + radius; x++)
            {

                for (int y = (int)position.Y - constraint; y <= (int)position.Y + constraint; y++)
                {

                    for (int z = (int)position.Z - radius; z <= (int)position.Z + radius; z++)
                    {

                        list.Add((x,y,z));

                    }

                }

            }

            return list.ToArray();

        }
        public static Vector3[] FilledColumn(Vector3 position, int radius, int radiusConstraint)
        {

            List<Vector3> Vectors = new List<Vector3>();
            
            for (int r = 0; r <= radius; r++)
            {

                foreach (Vector3 RingPosition in SingleRing(position, r))
                {

                    foreach (Vector3 ColumnPosition in Column(RingPosition, radiusConstraint))
                    {

                        Vectors.Add(ColumnPosition);

                    }

                }

            }

            return Vectors.Distinct().ToArray();

        }

        public static Vector3[] RingedColumn(Vector3 position, int radius, int radiusConstraint)
        {

            List<Vector3> Vectors = new List<Vector3>();

            foreach (Vector3 RingPosition in SingleRing(position, radius))
            {

                foreach (Vector3 ColumnPosition in Column(RingPosition, radiusConstraint))
                {

                    Vectors.Add(ColumnPosition);

                }

            }

            return Vectors.Distinct().ToArray();

        }
        public static Vector3[] RingedColumnPadded(Vector3 position, int radius, int radiusPadding, int radiusConstraint)
        {

            List<Vector3> Vectors = new List<Vector3>();

            for (int rPadding = 0; rPadding <= radiusPadding; rPadding++)
            {

                foreach (Vector3 RingPosition in SingleRing(position, radius + rPadding))
                {

                    foreach (Vector3 ColumnPosition in Column(RingPosition, radiusConstraint))
                    {

                        Vectors.Add(ColumnPosition);

                    }

                }

                /* foreach (Vector3 RingPosition in SingleRing(position, radius - rPadding))
                {

                    foreach (Vector3 ColumnPosition in Column(RingPosition, radiusConstraint))
                    {

                        Vectors.Add(ColumnPosition);

                    }

                } */

            }

            return Vectors.Distinct().ToArray();

        }


        public static Vector3[] GenerateLoopsFromRadius(Vector3 position, int radius)
        {

            List<Vector3> vectors = new List<Vector3>();

            for (int i = 0; i < radius; ++i)
            {

                vectors.AddRange(SingleRing(position, i));

            }

            return vectors.Distinct().ToArray();

        }

        public static Vector3 WorldPositionToChunkPosition(Vector3 position)
        {

            int x = (int) Math.Floor(position.X / Globals.ChunkSize);
            int y = (int)Math.Floor(position.Y / Globals.ChunkSize);
            int z = (int)Math.Floor(position.Z / Globals.ChunkSize);

            return (x, y, z);

        }

        public static Vector3[] Column(Vector3 position, int radiusConstraint)
        {

            List<Vector3> vectors = new List<Vector3>();
            
            for (int y = -radiusConstraint; y <= radiusConstraint; y++)
            {

                vectors.Add((position.X, position.Y + y, position.Z));

            }

            return vectors.Distinct().ToArray();

        }

        public static Vector3[] SingleRing(Vector3 position, int radius)
        {

            List<Vector3> vectors = new List<Vector3>();

            for (int x = -radius; x <= radius; x++)
            {

                vectors.Add((position.X - x, position.Y, position.Z + radius));
                vectors.Add((position.X - x, position.Y, position.Z - radius));
                vectors.Add((position.X + radius, position.Y, position.Z - x));
                vectors.Add((position.X - radius, position.Y, position.Z - x));
                
            }

            return vectors.Distinct().ToArray();

        }

        public static Vector3[] RingFromPosition(int radius, Vector3 position, int yOffset)
        {


            List<Vector3> tempList = new List<Vector3>();
            if (radius == 0)
            {

                tempList.Add((position.X, position.Y+yOffset, position.Z));
                tempList.Add((position.X, position.Y - yOffset, position.Z));

            }

            // tempList.Add((0, yOffset, 0));

            for (int i = (int)position.X - radius; i <= (int)position.X + radius; i++)
            {

                tempList.Add((i, position.Y + yOffset, position.Z + radius));
                tempList.Add((i, position.Y + yOffset, position.Z - radius));

                tempList.Add((i, position.Y - yOffset, position.Z + radius));
                tempList.Add((i, position.Y - yOffset, position.Z - radius));

            }

            for (int i = (int)position.Z - radius + 1; i < (int)position.Z + radius; i++)
            {

                tempList.Add((position.X + radius, position.Y + yOffset, i));
                tempList.Add((position.X - radius, position.Y + yOffset, i));

                tempList.Add((position.X + radius, position.Y - yOffset, i));
                tempList.Add((position.X - radius, position.Y - yOffset, i));

            }

            return tempList.ToArray();

        }

        public static Vector3[] HollowCubeFromPosition(Vector3 position, int radius)
        {

            List<Vector3> VectorList = new List<Vector3>();

            for (int o = 0; o <= radius; o++)
            {

                VectorList.AddRange(RingFromPosition(radius, position, o));

            }

            for (int x = (int)position.X - radius + 1; x  < (int)position.X+radius; x++)
            {

                for (int z = (int)position.Z - radius + 1; z < (int) position.Z+radius; z++)
                {

                    VectorList.Add((x,position.Y+radius,z));
                    VectorList.Add((x,position.Y-radius, z));

                }

            }

            return VectorList.ToArray();

        }

        public static Vector3[] Ring(Vector3 position, int radius, int offset)
        {

            List<Vector3> vectors = new List<Vector3>();

            for (int i = 0; i <= radius; i++)
            {

                vectors.Add((position.X-i, position.Y+offset, position.Z+radius));
                vectors.Add((position.X+i, position.Y + offset, position.Z + radius));

                vectors.Add((position.X - radius, position.Y + offset, position.Z - i));
                vectors.Add((position.X - radius, position.Y + offset, position.Z + i));

                vectors.Add((position.X - i, position.Y + offset, position.Z - radius));
                vectors.Add((position.X + i, position.Y + offset, position.Z - radius));

                vectors.Add((position.X + radius, position.Y + offset, position.Z - i));
                vectors.Add((position.X + radius, position.Y + offset, position.Z + i));

                vectors.Add((position.X - i, position.Y - offset, position.Z + radius));
                vectors.Add((position.X + i, position.Y - offset, position.Z + radius));

                vectors.Add((position.X - radius, position.Y - offset, position.Z - i));
                vectors.Add((position.X - radius, position.Y - offset, position.Z + i));

                vectors.Add((position.X - i, position.Y - offset, position.Z - radius));
                vectors.Add((position.X + i, position.Y - offset, position.Z - radius));

                vectors.Add((position.X + radius, position.Y - offset, position.Z - i));
                vectors.Add((position.X + radius, position.Y - offset, position.Z + i));

            }

            return vectors.Distinct().ToArray();

        }
        public static Vector3[] ColumnFromPosition(int radius, Vector3 position, int constraint)
        {

            List<Vector3> Vectors = new List<Vector3>();

            for (int i = 0; i <= constraint; i++)
            {

                Vectors.AddRange(Ring(position, radius, i));

            }

            return Vectors.ToArray();

        }


        public static Vector3 PositionToChunk(Vector3 position)
        {

            return ((float)Math.Floor(position.X/Globals.ChunkSize), (float)Math.Floor(position.Y/Globals.ChunkSize), (float)Math.Floor(position.Z/Globals.ChunkSize));

        }
        
        public static Vector3 PositionToChunkBounds(Vector3 position)
        {

            Vector3 PositionBlock = Vector3.Zero;

            if (position.X >= 0)
            {

                PositionBlock.X = position.X % Globals.ChunkSize;

            }
            if (position.Y >= 0)
            {

                PositionBlock.Y = position.Y % Globals.ChunkSize;

            }
            if (position.Z >= 0)
            {

                PositionBlock.Z = position.Z % Globals.ChunkSize;

            }

            if (position.X < 0)
            {

                PositionBlock.X = (Globals.ChunkSize) - Math.Abs(position.X) % Globals.ChunkSize;

            }
            if (position.Y < 0)
            {

                PositionBlock.Y = (Globals.ChunkSize) - Math.Abs(position.Y) % Globals.ChunkSize;

            }
            if (position.Z < 0)
            {

                PositionBlock.Z = (Globals.ChunkSize) - Math.Abs(position.Z) % Globals.ChunkSize;

            }

            return PositionBlock;

            // return (position.X % Globals.ChunkSize, position.Y % Globals.ChunkSize, position.Z % Globals.ChunkSize);

        }

        public static Vector3 PositionToBlockLocal(Vector3 position)
        {

            Vector3 PositionBlock = Vector3.Zero;

            if (position.X >= 0)
            {

                PositionBlock.X = (float)Math.Floor(position.X) % Globals.ChunkSize;

            }
            if (position.Y >= 0)
            {

                PositionBlock.Y = (float)Math.Floor(position.Y) % Globals.ChunkSize;

            }
            if (position.Z >= 0)
            {

                PositionBlock.Z = (float)Math.Floor(position.Z) % Globals.ChunkSize;

            }

            if (position.X < 0)
            {

                PositionBlock.X = (Globals.ChunkSize-1) - ((float)Math.Floor(Math.Abs(position.X)) % Globals.ChunkSize);

            }
            if (position.Y < 0)
            {

                PositionBlock.Y = (Globals.ChunkSize-1) - ((float)Math.Floor(Math.Abs(position.Y)) % Globals.ChunkSize);

            }
            if (position.Z < 0)
            {

                PositionBlock.Z = (Globals.ChunkSize-1) - ((float)Math.Floor(Math.Abs(position.Z)) % Globals.ChunkSize);

            }

            return PositionBlock;

            // return ((float)Math.Floor(position.X) % Globals.ChunkSize, (float)Math.Floor(position.Y) % Globals.ChunkSize, (float)Math.Floor(position.Z) % Globals.ChunkSize);

        }
        
        public static Vector3 PositionToBlockGlobal(Vector3 position)
        {

            return ((float)Math.Floor(position.X), (float)Math.Floor(position.Y), (float)Math.Floor(position.Z));

        }





        public static Vector3 getPlayerPositionRelativeToChunk(Vector3 position)
        {

            float x = Math.Max(0, (float)Math.Floor(position.X + 0.5f));
            float y = Math.Max(0, (float)Math.Floor(position.Y + 0.5f));
            float z = Math.Max(0, (float)Math.Floor(position.Z + 0.5f));

            x = Math.Min(Globals.ChunkSize - 1, x);
            y = Math.Min(Globals.ChunkSize - 1, y);
            z = Math.Min(Globals.ChunkSize - 1, z);

            return new Vector3(x, y, z);

        }

        public static Vector3 GetPositionRelativeToChunkPosition(Camera camera)
        {

            int x = (int)Math.Floor(camera.position.X / 32);
            int y = (int)Math.Floor(camera.position.Y / 32);
            int z = (int)Math.Floor(camera.position.Z / 32);

            return (x, y, z);

        }

    }
}
