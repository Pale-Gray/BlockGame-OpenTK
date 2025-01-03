using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using Blockgame_OpenTK.Util;
using System.Threading.Tasks;
using Blockgame_OpenTK.Core.Worlds;

namespace Blockgame_OpenTK.Core.Chunks
{
    internal class ChunkUtils
    {

        static int Steps = 0;
        static int FB = 1;
        static int LR = 0;
        static Vector3 SpiralOrigin = Vector3.Zero;
        static Vector3 SpiralPosition = SpiralOrigin;

        public static bool GetSolidBlock(uint[] solidMask, Vector3i blockPosition)
        {

            return (solidMask[VecToIndex((blockPosition.X, blockPosition.Z))] >> blockPosition.Y & 1u) == 1;

        }

        public static void SetSolidBlock(uint[] solidMask, Vector3i blockPosition, bool isSolid)
        {

            if (isSolid)
            {

                solidMask[VecToIndex((blockPosition.X, blockPosition.Z))] |= (1u << blockPosition.Y);

            } else
            {

                solidMask[VecToIndex((blockPosition.X, blockPosition.Z))] &= ~(1u << blockPosition.Y);

            }

        }

        public static int VecToIndex(Vector2i position)
        {

            return position.X + (position.Y * GlobalValues.ChunkSize);

        }

        public static int VecToIndex(Vector3i position)
        {

            return position.X + (position.Y * GlobalValues.ChunkSize) + (position.Z * GlobalValues.ChunkSize * GlobalValues.ChunkSize);

        }

        public static Dictionary<Vector3i, uint[]> GetChunkNeighborsSolidMaskDictionary(World world, Vector3i chunkPosition)
        {

            Dictionary<Vector3i, uint[]> neighborMasks = new();

            for (int x = -1; x <= 1; x++)
            {

                for (int y = -1; y <= 1; y++)
                {

                    for (int z  = -1; z <= 1; z++)
                    {

                        neighborMasks.Add((x, y, z), world.WorldChunks[(x, y, z) + chunkPosition].BitSolidMask);

                    }

                }
    
            }

            return neighborMasks;

        }

        public static bool TrySafePositionToBlockLocal(Vector3i chunkPosition, Vector3i globalBlockPosition, out Vector3i localBlockPosition)
        {

            Vector3i currentChunkPosition = PositionToChunk(globalBlockPosition);
            if (currentChunkPosition != chunkPosition)
            {

                localBlockPosition = Vector3i.Zero;
                return false;

            }
            localBlockPosition = PositionToBlockLocal(globalBlockPosition);
            return true;

        }

        public static Vector3i[] GenerateRingsOfColumnsWithPadding(int radius, int maxHeight, int padding)
        {

            List<Vector3i> vectors = new List<Vector3i>();

            for (int i = 0; i <= padding; i++)
            {

                vectors.AddRange(GenerateRingsOfColumns(radius + i, maxHeight));

            }

            return vectors.ToArray();

        }

        public static Vector3i[] GenerateColumns(int radius, int maxHeight)
        {

            List<Vector3i> vectors = new List<Vector3i>();

            for (int i = 0; i <= maxHeight; i++)
            {

                vectors.Add((0, i, radius));
                vectors.Add((0, -i, radius));
                vectors.Add((0, i, -radius));
                vectors.Add((0, -i, -radius));

                vectors.Add((-radius, i, 0));
                vectors.Add((radius, -i, 0));
                vectors.Add((-radius, -i, 0));
                vectors.Add((radius, i, 0));

            }

            return vectors.ToArray();

        }

        public static Vector3i[] GenerateRingsOfColumnsOffset(Vector3i offset, int radius, int maxHeight)
        {

            Vector3i[] ring = GenerateRing(radius);

            List<Vector3i> vectors = new List<Vector3i>();

            for (int i = 0; i < ring.Length; i++)
            {

                for (int c = 1; c <= maxHeight; c++)
                {

                    if (!vectors.Contains(ring[i] + offset)) vectors.Add(ring[i] + offset);
                    if (!vectors.Contains(ring[i] + (0, c, 0) + offset)) vectors.Add(ring[i] + (0, c, 0) + offset);
                    if (!vectors.Contains(ring[i] - (0, c, 0) + offset)) vectors.Add(ring[i] - (0, c, 0) + offset);

                }

            }

            return vectors.ToArray();

        }
        public static Vector3i[] GenerateRingsOfColumns(int radius, int maxHeight)
        {

            Vector3i[] ring = GenerateRing(radius);

            List<Vector3i> vectors = new List<Vector3i>();

            if (radius == 0)
            {

                for (int c = 0; c <= maxHeight; c++)
                {

                    if (!vectors.Contains(Vector3i.Zero)) vectors.Add(Vector3i.Zero);
                    if (!vectors.Contains((0, c, 0))) vectors.Add((0, c, 0));
                    if (!vectors.Contains((0, c, 0))) vectors.Add((0, c, 0));

                }

            }

            for (int i = 0; i < ring.Length; i++)
            {

                for (int c = 1; c <= maxHeight; c++)
                {

                    if (!vectors.Contains(ring[i])) vectors.Add(ring[i]);
                    if (!vectors.Contains(ring[i] + (0, c, 0))) vectors.Add(ring[i] + (0, c, 0));
                    if (!vectors.Contains(ring[i] - (0, c, 0))) vectors.Add(ring[i] - (0, c, 0));

                }

            }

            return vectors.ToArray();

        }
        public static Vector3i[] GenerateRing(int radius)
        {

            List<Vector3i> vectors = new List<Vector3i>();

            if (radius == 0)
            {

               return new Vector3i[] { Vector3i.Zero };

            }

            for (int i = 0; i <= radius; i++)
            {

                if (!vectors.Contains((i, 0, radius))) vectors.Add((i, 0, radius));
                if (!vectors.Contains((-i, 0, radius))) vectors.Add((-i, 0, radius));
                if (!vectors.Contains((i, 0, -radius))) vectors.Add((i, 0, -radius));
                if (!vectors.Contains((-i, 0, -radius))) vectors.Add((-i, 0, -radius));
                if (!vectors.Contains((radius, 0, i))) vectors.Add((radius, 0, i));
                if (!vectors.Contains((radius, 0, -i))) vectors.Add((radius, 0, -i));
                if (!vectors.Contains((-radius, 0, i))) vectors.Add((-radius, 0, i));
                if (!vectors.Contains((-radius, 0, -i))) vectors.Add((-radius, 0, -i));

            }

            return vectors.ToArray();

        }

        public static Vector3i[] FloodIterate(Vector3i[] currentVectors, int maxRadius)
        {

            List<Vector3i> vectorList = new List<Vector3i>(currentVectors);
            
            for (int i = 0; i < currentVectors.Length; i++)
            {

                if ((vectorList[i] + Vector3i.UnitX).X <= maxRadius && !vectorList.Contains((vectorList[i] + Vector3i.UnitX))) vectorList.Add((vectorList[i] + Vector3i.UnitX));
                if ((vectorList[i] - Vector3i.UnitX).X >= -maxRadius && !vectorList.Contains((vectorList[i] - Vector3i.UnitX))) vectorList.Add((vectorList[i] - Vector3i.UnitX));
                if ((vectorList[i] + Vector3i.UnitY).Y <= maxRadius && !vectorList.Contains((vectorList[i] + Vector3i.UnitY))) vectorList.Add((vectorList[i] + Vector3i.UnitY));
                if ((vectorList[i] - Vector3i.UnitY).Y >= -maxRadius && !vectorList.Contains((vectorList[i] - Vector3i.UnitY))) vectorList.Add((vectorList[i] - Vector3i.UnitY));
                if ((vectorList[i] + Vector3i.UnitZ).Z <= maxRadius && !vectorList.Contains((vectorList[i] + Vector3i.UnitZ))) vectorList.Add((vectorList[i] + Vector3i.UnitZ));
                if ((vectorList[i] - Vector3i.UnitZ).Z >= -maxRadius && !vectorList.Contains((vectorList[i] - Vector3i.UnitZ))) vectorList.Add((vectorList[i] - Vector3i.UnitZ));

            }
            
            // Console.Log(vectorList.Count());

            return vectorList.ToArray();

        }

        public static Vector3i[] InitialFloodFill(int maxRadius, int iterations)
        {

            List<Vector3i> vectorList = new List<Vector3i> { Vector3i.Zero };

            for (int it = 0; it < iterations; it++)
            {

                int count = vectorList.Count();
                for (int i = 0; i < count; i++)
                {

                    if ((vectorList[i] + Vector3i.UnitX).X <= maxRadius && !vectorList.Contains((vectorList[i] + Vector3i.UnitX))) vectorList.Add((vectorList[i] + Vector3i.UnitX));
                    if ((vectorList[i] - Vector3i.UnitX).X >= -maxRadius && !vectorList.Contains((vectorList[i] - Vector3i.UnitX))) vectorList.Add((vectorList[i] - Vector3i.UnitX));
                    if ((vectorList[i] + Vector3i.UnitY).Y <= maxRadius && !vectorList.Contains((vectorList[i] + Vector3i.UnitY))) vectorList.Add((vectorList[i] + Vector3i.UnitY));
                    if ((vectorList[i] - Vector3i.UnitY).Y >= -maxRadius && !vectorList.Contains((vectorList[i] - Vector3i.UnitY))) vectorList.Add((vectorList[i] - Vector3i.UnitY));
                    if ((vectorList[i] + Vector3i.UnitZ).Z <= maxRadius && !vectorList.Contains((vectorList[i] + Vector3i.UnitZ))) vectorList.Add((vectorList[i] + Vector3i.UnitZ));
                    if ((vectorList[i] - Vector3i.UnitZ).Z >= -maxRadius && !vectorList.Contains((vectorList[i] - Vector3i.UnitZ))) vectorList.Add((vectorList[i] - Vector3i.UnitZ));

                }

            }

            return vectorList.ToArray();

        }
        
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

            int x = (int) Math.Floor(position.X / GlobalValues.ChunkSize);
            int y = (int)Math.Floor(position.Y / GlobalValues.ChunkSize);
            int z = (int)Math.Floor(position.Z / GlobalValues.ChunkSize);

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
        public static Vector3i PositionToChunk(Vector3 position)
        {

            Vector3 globalBlockPosition = VectorMath.Floor(position);
            Vector3i chunkPosition = VectorMath.Floor(globalBlockPosition / 32.0f);

            return chunkPosition;

        }
        
        public static Vector3 PositionToChunkBounds(Vector3 position)
        {

            Vector3 PositionBlock = Vector3.Zero;

            if (position.X >= 0)
            {

                PositionBlock.X = position.X % GlobalValues.ChunkSize;

            }
            if (position.Y >= 0)
            {

                PositionBlock.Y = position.Y % GlobalValues.ChunkSize;

            }
            if (position.Z >= 0)
            {

                PositionBlock.Z = position.Z % GlobalValues.ChunkSize;

            }

            if (position.X < 0)
            {

                PositionBlock.X = (GlobalValues.ChunkSize) - Math.Abs(position.X) % GlobalValues.ChunkSize;

            }
            if (position.Y < 0)
            {

                PositionBlock.Y = (GlobalValues.ChunkSize) - Math.Abs(position.Y) % GlobalValues.ChunkSize;

            }
            if (position.Z < 0)
            {

                PositionBlock.Z = (GlobalValues.ChunkSize) - Math.Abs(position.Z) % GlobalValues.ChunkSize;

            }

            return PositionBlock;

            // return (position.X % Globals.ChunkSize, position.Y % Globals.ChunkSize, position.Z % Globals.ChunkSize);

        }

        public static Vector3i PositionToBlockLocal(Vector3i position)
        {

            position.X = Maths.Mod(position.X, 32);
            position.Y = Maths.Mod(position.Y, 32);
            position.Z = Maths.Mod(position.Z, 32);

            return position;


            /*
            Vector3 Position = PositionToBlockGlobal(position);

            Vector3 PositionBlock = Vector3.Zero;

            if (position.X >= 0)
            {

                PositionBlock.X = Position.X % Globals.ChunkSize;

            }
            if (position.Y >= 0)
            {

                PositionBlock.Y = Position.Y % Globals.ChunkSize;

            }
            if (position.Z >= 0)
            {

                PositionBlock.Z = Position.Z % Globals.ChunkSize;

            }

            if (position.X < 0)
            {

                
                PositionBlock.X = (Globals.ChunkSize-1) + (Position.X+1) % Globals.ChunkSize;


            }
            if (position.Y < 0)
            {

                PositionBlock.Y = (Globals.ChunkSize - 1) + (Position.Y + 1) % Globals.ChunkSize;

            }
            if (position.Z < 0)
            {

                PositionBlock.Z = (Globals.ChunkSize - 1) + (Position.Z + 1) % Globals.ChunkSize;

            }

            return (Vector3i) PositionBlock;
            */
            // return ((float)Math.Floor(position.X) % Globals.ChunkSize, (float)Math.Floor(position.Y) % Globals.ChunkSize, (float)Math.Floor(position.Z) % Globals.ChunkSize);

        }
        
        public static Vector3i PositionToBlockGlobal(Vector3 position)
        {

            return ((int)Math.Floor(position.X), (int)Math.Floor(position.Y), (int)Math.Floor(position.Z));

        }
        public static Vector3 getPlayerPositionRelativeToChunk(Vector3 position)
        {

            float x = Math.Max(0, (float)Math.Floor(position.X + 0.5f));
            float y = Math.Max(0, (float)Math.Floor(position.Y + 0.5f));
            float z = Math.Max(0, (float)Math.Floor(position.Z + 0.5f));

            x = Math.Min(GlobalValues.ChunkSize - 1, x);
            y = Math.Min(GlobalValues.ChunkSize - 1, y);
            z = Math.Min(GlobalValues.ChunkSize - 1, z);

            return new Vector3(x, y, z);

        }

        public static Vector3 GetPositionRelativeToChunkPosition(Camera camera)
        {

            int x = (int)Math.Floor(camera.Position.X / 32);
            int y = (int)Math.Floor(camera.Position.Y / 32);
            int z = (int)Math.Floor(camera.Position.Z / 32);

            return (x, y, z);

        }

    }
}
