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

        public static void SetLightValue(Chunk chunk, Vector3i lightPosition, ChunkLight light)
        {

            chunk.PackedLightData[VecToIndex(PositionToBlockLocal(lightPosition))] =
                (ushort) (light.ToPackedData() | (chunk.PackedLightData[VecToIndex(PositionToBlockLocal(lightPosition))] & 0x000F));

        }

        public static void SetLightRedValue(Chunk chunk, Vector3i lightPosition, ushort red)
        {

            ChunkLight c = ChunkLight.FromPackedData(chunk.PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(lightPosition))]);
            c.R = red;
            SetLightValue(chunk, lightPosition, c);
            
        }

        public static ushort GetLightRedValue(Chunk chunk, Vector3i lightPosition)
        {
            
            return (ushort) ((chunk.PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(lightPosition))] >> 12) & 15);
            
        }

        public static void SetLightGreenValue(Chunk chunk, Vector3i lightPosition, ushort green)
        {
            
            ChunkLight c = ChunkLight.FromPackedData(chunk.PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(lightPosition))]);
            c.G = green;
            SetLightValue(chunk, lightPosition, c);
            
        }
        
        public static ushort GetLightGreenValue(Chunk chunk, Vector3i lightPosition)
        {
            
            return (ushort) ((chunk.PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(lightPosition))] >> 8) & 15);
            
        }
        
        public static void SetLightBlueValue(Chunk chunk, Vector3i lightPosition, ushort blue)
        {
            
            ChunkLight c = ChunkLight.FromPackedData(chunk.PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(lightPosition))]);
            c.B = blue;
            SetLightValue(chunk, lightPosition, c);
            
        }
        
        public static ushort GetLightBlueValue(Chunk chunk, Vector3i lightPosition)
        {
            
            return (ushort) ((chunk.PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(lightPosition))] >> 4) & 15);
            
        }

        public static ChunkLight GetLightValue(Chunk chunk, Vector3i lightPosition)
        {

            return ChunkLight.FromPackedData(chunk.PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(lightPosition))]);

        }
        public static bool GetSolidBlock(Chunk chunk, Vector3i blockPosition)
        {

            blockPosition = ChunkUtils.PositionToBlockLocal(blockPosition);
            return (chunk.BitSolidMask[VecToIndex((blockPosition.X, blockPosition.Z))] >> blockPosition.Y & 1u) == 1;

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

        public static Dictionary<Vector3i, Chunk> GetChunkNeighbors(World world, Vector3i chunkPosition)
        {
            
            Dictionary<Vector3i, Chunk> neighbors = new();

            for (int x = -1; x <= 1; x++)
            {

                for (int y = -1; y <= 1; y++)
                {

                    for (int z  = -1; z <= 1; z++)
                    {

                        neighbors.Add((x, y, z), world.WorldChunks[(x, y, z) + chunkPosition]);

                    }

                }
    
            }

            return neighbors;
            
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
        
        public static Vector3i PositionToChunk(Vector3 position)
        {

            Vector3 globalBlockPosition = VectorMath.Floor(position);
            Vector3i chunkPosition = VectorMath.Floor(globalBlockPosition / 32.0f);

            return chunkPosition;

        }

        public static Vector3i PositionToBlockLocal(Vector3i position)
        {

            position.X = Maths.Mod(position.X, 32);
            position.Y = Maths.Mod(position.Y, 32);
            position.Z = Maths.Mod(position.Z, 32);

            return position;

        }
        
        public static Vector3i PositionToBlockGlobal(Vector3 position)
        {

            return ((int)Math.Floor(position.X), (int)Math.Floor(position.Y), (int)Math.Floor(position.Z));

        }

    }
}
