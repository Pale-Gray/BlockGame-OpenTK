using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using Blockgame_OpenTK.Util;
using System.Threading.Tasks;
using Blockgame_OpenTK.Core.Worlds;

namespace Blockgame_OpenTK.Core.Chunks
{
    public class ChunkUtils
    {

        static Vector3 SpiralOrigin = Vector3.Zero;

        public static LightColor GetLightColor(PackedChunk chunk, Vector3i localBlockPosition)
        {

            return new LightColor(chunk.LightData[VecToIndex(localBlockPosition)]);

        }

        public static ushort GetLightRedColor(PackedChunk chunk, Vector3i localBlockPosition)
        {

            return (ushort) (chunk.LightData[VecToIndex(localBlockPosition)] >> 12 & 15);

        }

        public static ushort GetLightGreenColor(PackedChunk chunk, Vector3i localBlockPosition) 
        {

            return (ushort) (chunk.LightData[VecToIndex(localBlockPosition)] >> 8 & 15);

        }

        public static ushort GetLightBlueColor(PackedChunk chunk, Vector3i localBlockPosition)
        {

            return (ushort) (chunk.LightData[VecToIndex(localBlockPosition)] >> 4 & 15);

        }

        public static void SetLightRedColor(PackedChunk chunk, Vector3i localBlockPosition, ushort redValue)
        {

            chunk.LightData[ChunkUtils.VecToIndex(localBlockPosition)] &= 0x0FFF;
            chunk.LightData[ChunkUtils.VecToIndex(localBlockPosition)] |= (ushort) (redValue << 12); 

        }

        public static void SetLightGreenColor(PackedChunk chunk, Vector3i localBlockPosition, ushort greenValue)
        {
            chunk.LightData[ChunkUtils.VecToIndex(localBlockPosition)] &= 0xF0FF;
            chunk.LightData[ChunkUtils.VecToIndex(localBlockPosition)] |= (ushort) (greenValue << 8);
        }

        public static void SetLightBlueColor(PackedChunk chunk, Vector3i localBlockPosition, ushort blueValue)
        {
            chunk.LightData[ChunkUtils.VecToIndex(localBlockPosition)] &= 0xFF0F;
            chunk.LightData[ChunkUtils.VecToIndex(localBlockPosition)] |= (ushort) (blueValue << 4);
        }

        public static void SetSunlightValue(PackedChunk chunk, Vector3i localBlockPosition, ushort sunValue)
        {
            chunk.LightData[ChunkUtils.VecToIndex(localBlockPosition)] &= 0xFFF0;
            chunk.LightData[ChunkUtils.VecToIndex(localBlockPosition)] |= sunValue;
        }

        public static ushort GetSunlightValue(PackedChunk chunk, Vector3i localBlockPosition)
        {
            return (ushort) (chunk.LightData[ChunkUtils.VecToIndex(localBlockPosition)] & 15);
        }

        public static void SetLightColor(PackedChunk chunk, Vector3i localBlockPosition, LightColor light) {

            ushort currentLight = chunk.LightData[VecToIndex(localBlockPosition)];
            chunk.LightData[VecToIndex(localBlockPosition)] = light.LightData;

        }

        public static void SubtractLightColor(PackedChunk chunk, Vector3i localBlockPosition, LightColor light)
        {

            LightColor currentLightColor = GetLightColor(chunk, localBlockPosition);
            currentLightColor -= light;
            SetLightColor(chunk, localBlockPosition, currentLightColor);

        }

        public static bool GetSolidBlock(PackedChunk chunk, Vector3i localBlockPosition) 
        {

            return (chunk.SolidMask[VecToIndex((localBlockPosition.X, localBlockPosition.Z))] & (1u << localBlockPosition.Y)) != 0;

        }

        public static void SetSolidBlock(PackedChunk chunk, Vector3i localBlockPosition, bool isSolid)
        {

            if (isSolid)
            {

                chunk.SolidMask[VecToIndex((localBlockPosition.X, localBlockPosition.Z))] |= (1u << localBlockPosition.Y);

            } else
            {

                chunk.SolidMask[VecToIndex((localBlockPosition.X, localBlockPosition.Z))] &= ~(1u << localBlockPosition.Y);

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

        public static Dictionary<Vector3i, PackedChunk> GetChunkNeighbors(PackedChunkWorld world, Vector3i chunkPosition)
        {
            
            Dictionary<Vector3i, PackedChunk> neighbors = new();

            for (int x = -1; x <= 1; x++)
            {

                for (int y = -1; y <= 1; y++)
                {

                    for (int z  = -1; z <= 1; z++)
                    {

                        neighbors.Add((x, y, z), world.PackedWorldChunks[(x, y, z) + chunkPosition]);

                    }

                }
    
            }

            return neighbors;
            
        }

        public static Dictionary<Vector3i, uint[]> GetChunkNeighborsSolidMaskDictionary(PackedChunkWorld world, Vector3i chunkPosition)
        {

            Dictionary<Vector3i, uint[]> neighborMasks = new();

            for (int x = -1; x <= 1; x++)
            {

                for (int y = -1; y <= 1; y++)
                {

                    for (int z  = -1; z <= 1; z++)
                    {

                        neighborMasks.Add((x, y, z), world.PackedWorldChunks[(x, y, z) + chunkPosition].SolidMask);

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
