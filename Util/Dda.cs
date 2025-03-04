using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Blockgame_OpenTK.Util
{
    public class Dda
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

        // private static List<Vector3i> _newLightPropagationPositions = new List<Vector3i>();
        public static void ComputeVisibility(World world, PackedChunk chunk, Vector3i globalLightPosition, Vector3i lightColor)
        {

            ComputeLightCorner(world, chunk, globalLightPosition, lightColor, (1, 1, 1));
            ComputeLightCorner(world, chunk, globalLightPosition, lightColor, (-1, 1, 1));
            ComputeLightCorner(world, chunk, globalLightPosition, lightColor, (-1, 1, -1));
            ComputeLightCorner(world, chunk, globalLightPosition, lightColor, (1, 1, -1));

            ComputeLightCorner(world, chunk, globalLightPosition, lightColor, (1, -1, 1));
            ComputeLightCorner(world, chunk, globalLightPosition, lightColor, (-1, -1, 1));
            ComputeLightCorner(world, chunk, globalLightPosition, lightColor, (-1, -1, -1));
            ComputeLightCorner(world, chunk, globalLightPosition, lightColor, (1, -1, -1));

        }

        public static void ComputeSunlightVisibility(World world, PackedChunk chunk, Vector3i position)
        {

            ComputeSunlightCorner(world, chunk, position, (1, 1));
            ComputeSunlightCorner(world, chunk, position, (-1, 1));
            ComputeSunlightCorner(world, chunk, position, (1, -1));
            ComputeSunlightCorner(world, chunk, position, (-1, -1));

        }

        private static void ComputeSunlightCorner(World world, PackedChunk chunk, Vector3i globalLightPosition, Vector2i direction)
        {

            uint[] expectedSunlightValues = new uint[15 * 15];
            uint[] actualSunlightValues = new uint[15 * 15];

            int signX = int.Sign(direction.X);
            int signZ = int.Sign(direction.Y);

            for (int x = 0; Math.Abs(x) < 15; x += signX * 1)
            {

                for (int z = 0; Math.Abs(z) < 15; z += signZ * 1)
                {

                    Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalLightPosition + (x, 0, z));
                    Vector3i localLightPosition = ChunkUtils.PositionToBlockLocal(globalLightPosition + (x, 0, z));

                    if (x == 0 && z == 0)
                    {

                        expectedSunlightValues[Maths.VecToIndex(Math.Abs(x), z, 15)] = 15;
                        actualSunlightValues[Maths.VecToIndex(Math.Abs(x), z, 15)] = 15;

                        // expectedLightValues[Maths.VecToIndex(Math.Abs(x), y, z, max)] = normalizedLightColor;
                        // actualLightValues[Maths.VecToIndex(Math.Abs(x), y, z, max)] = normalizedLightColor;

                    }
                    else
                    {

                        uint leftExpectedSunlightData = 0;
                        uint forwardExpectedSunlightData = 0;

                        uint leftActualSunlightData = 0;
                        uint forwardActualSunlightData = 0;

                        // if (Math.Abs(x) - 1 >= 0 && !world.WorldChunks[chunkPosition].SolidMask[ChunkUtils.VecToIndex(localLightPosition)]) leftActualSunlightData = actualSunlightValues[Maths.VecToIndex(Math.Abs(x) - 1, Math.Abs(z), 15)];
                        // if (Math.Abs(y) - 1 >= 0 && !world.WorldChunks[chunkPosition].SolidMask[ChunkUtils.VecToIndex(localLightPosition)]) upActualLightData = actualLightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(y) - 1, Math.Abs(z), max)];
                        // if (Math.Abs(z) - 1 >= 0 && !world.WorldChunks[chunkPosition].SolidMask[ChunkUtils.VecToIndex(localLightPosition)]) forwardActualSunlightData = actualSunlightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(z) - 1, 15)];

                        if (Math.Abs(x) - 1 >= 0) leftExpectedSunlightData = expectedSunlightValues[Maths.VecToIndex(Math.Abs(x) - 1, Math.Abs(z), 15)];
                        // if (Math.Abs(y) - 1 >= 0) upExpectedLightData = expectedLightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(y) - 1, Math.Abs(z), max)];
                        if (Math.Abs(z) - 1 >= 0) forwardExpectedSunlightData = expectedSunlightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(z) - 1, 15)];

                        // actualLightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(y), Math.Abs(z), max)] = leftActualLightData + upActualLightData + forwardActualLightData;
                        actualSunlightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(z), 15)] = leftActualSunlightData + forwardActualSunlightData;
                        // expectedLightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(y), Math.Abs(z), max)] = leftExpectedLightData + upActualLightData + forwardExpectedLightData;
                        expectedSunlightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(z), 15)] = leftExpectedSunlightData + forwardExpectedSunlightData;


                    }

                    Vector3i safeLocalLightPosition = ChunkUtils.PositionToBlockLocal(globalLightPosition + (x, 0, z));

                    // Vector3 expectedLightValue = expectedLightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(y), Math.Abs(z), max)];
                    // Vector3 actualLightValue = actualLightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(y), Math.Abs(z), max)];

                    uint expectedSunlightValue = expectedSunlightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(z), 15)];
                    uint actualSunlightValue = actualSunlightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(z), 15)];
                    // Vector3 value = (actualLightValue / expectedLightValue);
                    // Console.WriteLine($"{actualSunlightValue}. {expectedSunlightValue}");
                    float resultingSunlightValue = ((float)actualSunlightValue / (float)expectedSunlightValue);
                    // Console.WriteLine($"{resultingSunlightValue}");

                    int d = (int)(Math.Ceiling(Maths.Dist3D(Vector3i.Zero, (x, 0, z))));
                    float dist = Maths.Dist3D(Vector3i.Zero, (x, 0, z));

                    // value *= (15 - dist) / 15.0f;
                    resultingSunlightValue *= (15 - dist) / 15.0f;

                    uint finalSunlightValue = (uint) Math.Clamp(Math.Floor(resultingSunlightValue * 15), 0, 15);
                    finalSunlightValue = Math.Clamp(finalSunlightValue, 0, 15);
                    // Vector3i final = Vector3i.Clamp(VectorMath.Floor(value * lightColor), Vector3i.Zero, (15, 15, 15));
                    // final = Vector3i.Clamp(final, Vector3i.Zero, (15, 15, 15));
                    // uint previousSunlightValue = world.WorldChunks[chunkPosition].PackedLightData[ChunkUtils.VecToIndex(safeLocalLightPosition)] & 15;
                    // Console.WriteLine(previousSunlightValue);

                    Vector3i previousUnpackedLightValue = BlockLightColorConverter.Unpack(world.PackedWorldChunks[chunkPosition].LightData[ChunkUtils.VecToIndex(safeLocalLightPosition)]);
                    ushort currentSunLight = (ushort)(world.PackedWorldChunks[chunkPosition].LightData[ChunkUtils.VecToIndex(safeLocalLightPosition)] & 0b0000000000001111);

                    // world.WorldChunks[chunkPosition].PackedLightData[ChunkUtils.VecToIndex(safeLocalLightPosition)] = (ushort)(BlockLightColorConverter.Pack(Vector3i.ComponentMax(previousUnpackedLightValue, final)) | currentSunLight);
                    // Console.WriteLine(finalSunlightValue);
                    // uint maxSunlight = Math.Max(previousSunlightValue, finalSunlightValue);
                    // Console.WriteLine($"prev: {previousSunlightValue}, current: {finalSunlightValue}, finish: {maxSunlight}");
                    // world.WorldChunks[chunkPosition].PackedLightData[ChunkUtils.VecToIndex(safeLocalLightPosition)] = (world.WorldChunks[chunkPosition].PackedLightData[ChunkUtils.VecToIndex(safeLocalLightPosition)] & 0xFFFFFFF0) | maxSunlight;
                    // world.WorldChunks[chunkPosition].PackedLightData[ChunkUtils.VecToIndex(safeLocalLightPosition)] = 
                    //  = (ushort)(BlockLightColorConverter.Pack(Vector3i.ComponentMax(previousUnpackedLightValue, final)) | currentSunLight);

                }

            }

        }

        private static void ComputeLightCorner(World world, PackedChunk chunk, Vector3i globalLightPosition, Vector3i lightColor, Vector3i direction)
        {

            int max = Math.Max(lightColor.X, Math.Max(lightColor.Y, lightColor.Z)) + 1;

            Vector3[] expectedLightValues = new Vector3[max*max*max];
            Vector3[] actualLightValues = new Vector3[max*max*max];

            Vector3 normalizedLightColor = (Vector3)lightColor / 15.0f;

            int signX = int.Sign(direction.X);
            int signY = int.Sign(direction.Y);
            int signZ = int.Sign(direction.Z);

            for (int x = 0; Math.Abs(x) < max; x += signX * 1) 
            {

                for (int y = 0; Math.Abs(y) < max; y += signY * 1)
                {

                    for (int z = 0; Math.Abs(z) < max; z += signZ * 1)
                    {

                        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalLightPosition + (x, y, z));
                        Vector3i localLightPosition = ChunkUtils.PositionToBlockLocal(globalLightPosition + (x, y, z));

                        if (x == 0 && y == 0 && z == 0)
                        {

                            expectedLightValues[Maths.VecToIndex(Math.Abs(x), y, z, max)] = normalizedLightColor;
                            actualLightValues[Maths.VecToIndex(Math.Abs(x), y, z, max)] = normalizedLightColor;

                        }
                        else
                        {

                            Vector3 leftExpectedLightData = Vector3.Zero;
                            Vector3 forwardExpectedLightData = Vector3.Zero;
                            Vector3 upExpectedLightData = Vector3.Zero;

                            Vector3 leftActualLightData = Vector3.Zero;
                            Vector3 forwardActualLightData = Vector3.Zero;
                            Vector3 upActualLightData = Vector3.Zero;

                            // if (Math.Abs(x) - 1 >= 0 && !world.WorldChunks[chunkPosition].SolidMask[ChunkUtils.VecToIndex(localLightPosition)]) leftActualLightData = actualLightValues[Maths.VecToIndex(Math.Abs(x) - 1, Math.Abs(y), Math.Abs(z), max)];
                            // if (Math.Abs(y) - 1 >= 0 && !world.WorldChunks[chunkPosition].SolidMask[ChunkUtils.VecToIndex(localLightPosition)]) upActualLightData = actualLightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(y) - 1, Math.Abs(z), max)];
                            // if (Math.Abs(z) - 1 >= 0 && !world.WorldChunks[chunkPosition].SolidMask[ChunkUtils.VecToIndex(localLightPosition)]) forwardActualLightData = actualLightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(y), Math.Abs(z) - 1, max)];

                            if (Math.Abs(x) - 1 >= 0) leftExpectedLightData = expectedLightValues[Maths.VecToIndex(Math.Abs(x) - 1, Math.Abs(y), Math.Abs(z), max)];
                            if (Math.Abs(y) - 1 >= 0) upExpectedLightData = expectedLightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(y) - 1, Math.Abs(z), max)];
                            if (Math.Abs(z) - 1 >= 0) forwardExpectedLightData = expectedLightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(y), Math.Abs(z) - 1, max)];

                            actualLightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(y), Math.Abs(z), max)] = leftActualLightData + upActualLightData + forwardActualLightData;
                            expectedLightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(y), Math.Abs(z), max)] = leftExpectedLightData + upActualLightData + forwardExpectedLightData;

                        }

                        Vector3i safeLocalLightPosition = ChunkUtils.PositionToBlockLocal(globalLightPosition + (x, y, z));

                        Vector3 expectedLightValue = expectedLightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(y), Math.Abs(z), max)];
                        Vector3 actualLightValue = actualLightValues[Maths.VecToIndex(Math.Abs(x), Math.Abs(y), Math.Abs(z), max)];

                        float fac = 0.2f;
                        Vector3 value = (actualLightValue / expectedLightValue);

                        int d = (int)(Math.Ceiling(Maths.Dist3D(Vector3i.Zero, (x, y, z))));
                        float dist = Maths.Dist3D(Vector3i.Zero, (x,y,z));

                        value *= (15 - dist) / 15.0f;

                        Vector3i final = Vector3i.Clamp(VectorMath.Floor(value * lightColor), Vector3i.Zero, (15, 15, 15));
                        final = Vector3i.Clamp(final, Vector3i.Zero, (15, 15, 15));

                        Vector3i previousUnpackedLightValue = BlockLightColorConverter.Unpack(world.PackedWorldChunks[chunkPosition].LightData[ChunkUtils.VecToIndex(safeLocalLightPosition)]);
                        ushort currentSunLight = (ushort)(world.PackedWorldChunks[chunkPosition].LightData[ChunkUtils.VecToIndex(safeLocalLightPosition)] & 0b0000000000001111);

                        // world.WorldChunks[chunkPosition].PackedLightData[ChunkUtils.VecToIndex(safeLocalLightPosition)] = (ushort)(BlockLightColorConverter.Pack(Vector3i.ComponentMax(previousUnpackedLightValue, final)) | currentSunLight);
                        // world.WorldChunks[chunkPosition].PackedLightData[ChunkUtils.VecToIndex(safeLocalLightPosition)] = world.WorldChunks[chunkPosition].PackedLightData[ChunkUtils.VecToIndex(safeLocalLightPosition)] | BlockLightColorConverter.Pack(Vector3i.ComponentMax(previousUnpackedLightValue, final));
                        //  = (ushort)(BlockLightColorConverter.Pack(Vector3i.ComponentMax(previousUnpackedLightValue, final)) | currentSunLight);

                    }

                }

            }

        }

        public static void ComputeLight(World world, Vector3i chunkPosition, Vector3i globalLightPosition, Vector3i lightColor)
        {

            Vector3i currentLightColor = lightColor;

            List<Vector3i> _newLightPropagationPositions = new List<Vector3i>()
            {

                globalLightPosition + (0, 0, 1),
                globalLightPosition + (0, 0, -1),
                globalLightPosition + (0, 1, 0),
                globalLightPosition + (0, -1, 0),
                globalLightPosition + (1, 0, 0),
                globalLightPosition + (-1, 0, 0)

            };

            if (ChunkUtils.TrySafePositionToBlockLocal(chunkPosition, globalLightPosition, out Vector3i lightPos))
            {

                // world.WorldChunks[chunkPosition].PackedLightData[ChunkUtils.VecToIndex(lightPos)] = BlockLightColorConverter.Pack(currentLightColor);

            }
            
            currentLightColor = Vector3i.ComponentMax(currentLightColor - Vector3i.One, Vector3i.Zero);
            
            while (currentLightColor != Vector3i.Zero)
            {

                List<Vector3i> currentLightPositions = _newLightPropagationPositions.ToList();

                foreach (Vector3i lightPosition in currentLightPositions)
                {

                    if (ChunkUtils.TrySafePositionToBlockLocal(chunkPosition, lightPosition, out Vector3i localLightPosition))
                    {

                        // if (!world.WorldChunks[chunkPosition].SolidMask[ChunkUtils.VecToIndex(localLightPosition)])
                        {

                            Vector3i previousLightColor = BlockLightColorConverter.Unpack(world.PackedWorldChunks[chunkPosition].LightData[ChunkUtils.VecToIndex(localLightPosition)]);
                            // world.WorldChunks[chunkPosition].PackedLightData[ChunkUtils.VecToIndex(localLightPosition)] = BlockLightColorConverter.Pack(Vector3i.ComponentMax(previousLightColor, currentLightColor));

                            _newLightPropagationPositions.Add(lightPosition + (0, 0, 1));
                            _newLightPropagationPositions.Add(lightPosition + (0, 0, -1));
                            _newLightPropagationPositions.Add(lightPosition + (0, 1, 0));
                            _newLightPropagationPositions.Add(lightPosition + (0, -1, 0));
                            _newLightPropagationPositions.Add(lightPosition + (1, 0, 0));
                            _newLightPropagationPositions.Add(lightPosition + (-1, 0, 0));

                        }

                    }

                    _newLightPropagationPositions = _newLightPropagationPositions.Except(currentLightPositions).ToList();

                }

                currentLightColor = Vector3i.ComponentMax(currentLightColor - Vector3i.One, Vector3i.Zero);

            }

            /*
            while (currentLightColor != Vector3i.Zero)
            {

                List<Vector3i> currentLightPositions = _newLightPropagationPositions;
                _newLightPropagationPositions.Clear();

                foreach (Vector3i lightPosition in currentLightPositions)
                {

                    if (ChunkUtils.TrySafePositionToBlockLocal(chunkPosition, lightPosition, out Vector3i localLightPosition))
                    {

                        Vector3i currentUnpackedLight = BlockLightColorConverter.Unpack(world.WorldChunks[chunkPosition].PackedLightData[ChunkUtils.VecToIndex(localLightPosition)]);
                        world.WorldChunks[chunkPosition].PackedLightData[ChunkUtils.VecToIndex(localLightPosition)] = BlockLightColorConverter.Pack(Vector3i.ComponentMax(currentUnpackedLight, currentLightColor));

                        _newLightPropagationPositions.Add(lightPosition + (0, 0, 1));
                        _newLightPropagationPositions.Add(lightPosition + (0, 0, -1));
                        _newLightPropagationPositions.Add(lightPosition + (0, 1, 0));
                        _newLightPropagationPositions.Add(lightPosition + (0, -1, 0));
                        _newLightPropagationPositions.Add(lightPosition + (1, 0, 0));
                        _newLightPropagationPositions.Add(lightPosition + (-1, 0, 0));

                    }

                }

                currentLightColor = Vector3i.ComponentMax(currentLightColor - Vector3i.One, Vector3i.Zero);

            }
            */

        }

        public static void ComputeLightRay(World world, Vector3i chunkPosition, Vector3i globalLightPosition, Vector3 direction, uint color)
        {

            // world.WorldChunks[ChunkUtils.PositionToChunk(globalLightPosition)].PackedLightData = new uint[GlobalValues.ChunkSize * GlobalValues.ChunkSize * GlobalValues.ChunkSize];
            uint currentR = color >> 12 & 0b1111;
            uint currentG = color >> 8 & 0b1111;
            uint currentB = color >> 4 & 0b1111;
            uint r = currentR;
            uint g = currentG;
            uint b = currentB;

            direction.Normalize();
            float Distance = 0;
            Vector3i currentGlobalBlockPosition = globalLightPosition;
            Vector3i step = Vector3i.Zero;
            Vector3 sideDistance = Vector3.Zero;
            Vector3 deltaDistance = Vector3.Abs(Vector3.One / direction);

            Vector3 offsetLightPosition = new Vector3(globalLightPosition.X + 0.5f, globalLightPosition.Y + 0.5f, globalLightPosition.Z + 0.5f);

            uint previousPackedLightData = world.PackedWorldChunks[ChunkUtils.PositionToChunk(currentGlobalBlockPosition)].LightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(currentGlobalBlockPosition))];

            // world.WorldChunks[ChunkUtils.PositionToChunk(currentGlobalBlockPosition)].PackedLightData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(currentGlobalBlockPosition))] = Math.Max(color, previousPackedLightData);

            if (direction.X < 0)
            {

                step.X = -1;
                sideDistance.X = (offsetLightPosition.X - globalLightPosition.X) * deltaDistance.X;

            }
            if (direction.X >= 0)
            {

                step.X = 1;
                sideDistance.X = (globalLightPosition.X + 1f - offsetLightPosition.X) * deltaDistance.X;

            }
            if (direction.Y < 0)
            {

                step.Y = -1;
                sideDistance.Y = (offsetLightPosition.Y - globalLightPosition.Y) * deltaDistance.Y;

            }
            if (direction.Y >= 0)
            {

                step.Y = 1;
                sideDistance.Y = (globalLightPosition.Y + 1f - offsetLightPosition.Y) * deltaDistance.Y;

            }
            if (direction.Z < 0)
            {

                step.Z = -1;
                sideDistance.Z = (offsetLightPosition.Z - globalLightPosition.Z) * deltaDistance.Z;

            }
            if (direction.Z >= 0)
            {

                step.Z = 1;
                sideDistance.Z = (globalLightPosition.Z + 1f - offsetLightPosition.Z) * deltaDistance.Z;

            }

            uint currentColor = 0;

            while (Maths.ChebyshevDistance3D(offsetLightPosition, currentGlobalBlockPosition) <= 16)
            {

                Vector3i lightChunkPosition = ChunkUtils.PositionToChunk(currentGlobalBlockPosition);
                Vector3i localLightPosition = ChunkUtils.PositionToBlockLocal(currentGlobalBlockPosition);

                // if (world.WorldChunks[lightChunkPosition].SolidMask[ChunkUtils.VecToIndex(localLightPosition)])
                {

                    if (globalLightPosition - currentGlobalBlockPosition != Vector3i.Zero)
                    {

                        break;

                    }

                }

                currentColor = 0;
                currentColor = currentR << 12 | currentColor;
                currentColor = currentG << 8 | currentColor;
                currentColor = currentB << 4 | currentColor;

                // uint current = world.WorldChunks[ChunkUtils.PositionToChunk(currentGlobalBlockPosition)].PackedLightData[ChunkUtils.VecToIndex(localLightPosition)];
                // world.WorldChunks[ChunkUtils.PositionToChunk(currentGlobalBlockPosition)].PackedLightData[ChunkUtils.VecToIndex(localLightPosition)] = Math.Max(current, currentColor);

                if (ChunkUtils.TrySafePositionToBlockLocal(chunkPosition, currentGlobalBlockPosition, out Vector3i lightPos))
                {

                    // Console.WriteLine(lightChunkPosition

                    uint previousLightDataPacked = world.PackedWorldChunks[lightChunkPosition].LightData[ChunkUtils.VecToIndex(localLightPosition)];

                    Vector3i previousUnpacked = Vector3i.Zero; // BlockLightColorConverter.Unpack(previousLightDataPacked);
                    Vector3i currentUnpacked = Vector3i.Zero; // BlockLightColorConverter.Unpack(currentColor);

                    Vector3i newUnpaced = Vector3i.Clamp(previousUnpacked + currentUnpacked, (0, 0, 0), (15, 15, 15));
                    if (previousUnpacked == currentUnpacked) newUnpaced = currentUnpacked;

                    Vector3 floatPrevious = previousUnpacked;
                    Vector3 floatCurrent = currentUnpacked;

                    // if (floatPrevious == Vector3.Zero) floatPrevious = (15, 15, 15);
                    floatPrevious /= 15.0f;
                    floatCurrent /= 15.0f;

                    Vector3 averagedColor = Vector3.Lerp(floatPrevious, floatCurrent, 0.5f);

                    Vector3 floatFac = Vector3.Clamp(floatPrevious * floatCurrent, Vector3.Zero, Vector3.One);
                    if (floatPrevious == floatCurrent) floatFac = floatCurrent;
                    Vector3i newUnpackedLight = VectorMath.Floor((averagedColor * 15.0f));


                    // if (previousUnpacked == Vector3i.Zero) previousUnpacked = (15, 15, 15);
                    // Vector3 floatValues = Vector3.ComponentMax(previousUnpacked, (15, 15, 15));
                    // Vector3 floatCurrentValues = new Vector3(currentUnpacked.X, currentUnpacked.Y, currentUnpacked.Z) / 15.0f;

                    // Vector3i mul = Vector3i.Clamp(VectorMath.Floor((floatValues * floatCurrentValues) / (15, 15, 15)), Vector3i.Zero, (15, 15, 15));

                    // Vector3i add = ((int)Math.Floor((previousUnpacked.X + currentUnpacked.X) / 2.0f), (int)Math.Floor((previousUnpacked.Y + currentUnpacked.Y) / 2.0f), (int)Math.Floor((previousUnpacked.Y + currentUnpacked.Y) / 2.0f));
                    // uint newData = BlockLightColorConverter.Pack(Vector3i.ComponentMax(previousUnpacked, currentUnpacked));
                    // uint newData = BlockLightColorConverter.Pack(Vector3i.ComponentMax(previousUnpacked, currentUnpacked));
                    uint newData = BlockLightColorConverter.Pack(Vector3i.ComponentMax(previousUnpacked, currentUnpacked));

                    uint currentSunLightData = previousLightDataPacked & 0b0000000000001111;
                    newData = newData | currentSunLightData;

                    // uint newData = BlockLightColorConverter.Pack(blocklightData);
                    // uint newData = BlockLightColorConverter.Pack(mul);

                    uint current = world.PackedWorldChunks[lightChunkPosition].LightData[ChunkUtils.VecToIndex(localLightPosition)];
                    // world.WorldChunks[chunkPosition].PackedLightData[ChunkUtils.VecToIndex(localLightPosition)] = newData;
                    // world.WorldChunks[lightChunkPosition].PackedLightData[ChunkUtils.VecToIndex(localLightPosition)] = Math.Max(current, currentColor);

                    // current = world.WorldChunks[ChunkUtils.PositionToChunk(globalLightPosition)].PackedLightData[ChunkUtils.VecToIndex(lightPos)];
                    // world.WorldChunks[ChunkUtils.PositionToChunk(globalLightPosition)].PackedLightData[ChunkUtils.VecToIndex(lightPos)] = Math.Max(current, currentColor);

                }

                if (currentR > 0) currentR--;
                if (currentG > 0) currentG--;
                if (currentB > 0) currentB--;

                if (currentR == 0 && currentG == 0 && currentB == 0) break;

                if (sideDistance.X < sideDistance.Y)
                {

                    if (sideDistance.X < sideDistance.Z)
                    {

                        Distance = sideDistance.X;
                        sideDistance.X += deltaDistance.X;

                        currentGlobalBlockPosition.X += step.X;


                    }
                    else
                    {

                        Distance = sideDistance.Z;
                        sideDistance.Z += deltaDistance.Z;

                        currentGlobalBlockPosition.Z += step.Z;

                    }


                }
                else
                {

                    if (sideDistance.Y < sideDistance.Z)
                    {

                        Distance = sideDistance.Y;
                        sideDistance.Y += deltaDistance.Y;

                        currentGlobalBlockPosition.Y += step.Y;

                    }
                    else
                    {

                        Distance = sideDistance.Z;
                        sideDistance.Z += sideDistance.Z;

                        currentGlobalBlockPosition.Z += step.Z;

                    }


                }

            }

        }

            /*
             * 
             * Implemented based on a few links
             * https://www.researchgate.net/publication/233899848_Efficient_implementation_of_the_3D-DDA_ray_traversal_algorithm_on_GPU_and_its_application_in_radiation_dose_calculation
             * https://www.shadertoy.com/view/4dX3zl
             * 
             */
            public static void TraceChunks(ConcurrentDictionary<Vector2i, ChunkColumn> chunkDictionary, Vector3 position, Vector3 direction, int maxSteps)
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

            // Console.Log("start");
            hit = false;

            while (Maths.ChebyshevDistance3D(position, GlobalBlockPosition) < maxSteps && !hit)
            {

                Vector3i chunkPosition = ChunkUtils.PositionToChunk(GlobalBlockPosition);
                if (chunkPosition.Y < PackedWorldGenerator.WorldGenerationHeight && chunkPosition.Y >= 0 && chunkDictionary.ContainsKey(chunkPosition.Xz) && !chunkDictionary[chunkPosition.Xz].Chunks[chunkPosition.Y].HasUpdates)
                {
                        
                    // if its not air...
                    if (chunkDictionary[ChunkUtils.PositionToChunk(GlobalBlockPosition).Xz].Chunks[ChunkUtils.PositionToChunk(GlobalBlockPosition).Y].BlockData[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(GlobalBlockPosition))] != 0)
                    {

                        // ChunkAtHit = (Vector3i)ChunkUtils.PositionToChunk(RoundedPosition);
                        // PositionAtHit = RoundedPosition;
                        ChunkAtHit = (Vector3i)ChunkUtils.PositionToChunk(GlobalBlockPosition);
                        hit = true;
                        PositionAtHit = GlobalBlockPosition;
                        SmoothPosition = position + NormalizedDirection * Distance;

                    } else
                    {

                        PreviousPositionAtHit = GlobalBlockPosition;
                        if (SideDistance.X < SideDistance.Y)
                        {
                            // PreviousPositionAtHit = GlobalBlockPosition;
                            if (SideDistance.X < SideDistance.Z)
                            {

                                Distance = SideDistance.X;
                                SideDistance.X += DeltaDistance.X;
                                // PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.X += Step.X;

                            }
                            else
                            {

                                Distance = SideDistance.Z;
                                SideDistance.Z += DeltaDistance.Z;
                                // PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.Z += Step.Z;

                            }
                            // PreviousPositionAtHit = GlobalBlockPosition;

                        }
                        else
                        {

                            PreviousPositionAtHit = GlobalBlockPosition;
                            if (SideDistance.Y < SideDistance.Z)
                            {

                                Distance = SideDistance.Y;
                                SideDistance.Y += DeltaDistance.Y;
                                // PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.Y += Step.Y;

                            }
                            else
                            {

                                Distance = SideDistance.Z;
                                SideDistance.Z += DeltaDistance.Z;
                                // PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.Z += Step.Z;

                            }
                            //PreviousPositionAtHit = GlobalBlockPosition;

                        }

                    }

                } else
                {

                    PreviousPositionAtHit = GlobalBlockPosition;
                    if (SideDistance.X < SideDistance.Y)
                    {
                        // PreviousPositionAtHit = GlobalBlockPosition;
                        if (SideDistance.X < SideDistance.Z)
                        {

                            Distance = SideDistance.X;
                            SideDistance.X += DeltaDistance.X;
                            // PreviousPositionAtHit = GlobalBlockPosition;
                            GlobalBlockPosition.X += Step.X;

                        }
                        else
                        {

                            Distance = SideDistance.Z;
                            SideDistance.Z += DeltaDistance.Z;
                            // PreviousPositionAtHit = GlobalBlockPosition;
                            GlobalBlockPosition.Z += Step.Z;

                        }
                        // PreviousPositionAtHit = GlobalBlockPosition;

                    }
                    else
                    {

                        PreviousPositionAtHit = GlobalBlockPosition;
                        if (SideDistance.Y < SideDistance.Z)
                        {

                            Distance = SideDistance.Y;
                            SideDistance.Y += DeltaDistance.Y;
                            // PreviousPositionAtHit = GlobalBlockPosition;
                            GlobalBlockPosition.Y += Step.Y;

                        }
                        else
                        {

                            Distance = SideDistance.Z;
                            SideDistance.Z += DeltaDistance.Z;
                            // PreviousPositionAtHit = GlobalBlockPosition;
                            GlobalBlockPosition.Z += Step.Z;

                        }
                        //PreviousPositionAtHit = GlobalBlockPosition;

                    }

                }


            }


            /*
                // if (ChunkLoader.ContainsGeneratedChunk(ChunkUtils.PositionToChunk(GlobalBlockPosition)))
            if (chunkDictionary.ContainsKey(ChunkUtils.PositionToChunk(GlobalBlockPosition)) && chunkDictionary[ChunkUtils.PositionToChunk(GlobalBlockPosition)].QueueType == QueueType.Done)
            {

                while (Maths.ChebyshevDistance3D(position, GlobalBlockPosition) < maxSteps && !hit)
                {

                    // if (ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(GlobalBlockPosition)).GetBlock(ChunkUtils.PositionToBlockLocal(GlobalBlockPosition)) != Blocks.AirBlock)
                    if (chunkDictionary[ChunkUtils.PositionToChunk(GlobalBlockPosition)].GetBlock(ChunkUtils.PositionToBlockLocal(GlobalBlockPosition)) != Blocks.AirBlock)
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
                                // PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.X += Step.X;

                            }
                            else
                            {

                                Distance = SideDistance.Z;
                                SideDistance.Z += DeltaDistance.Z;
                                // PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.Z += Step.Z;

                            }
                            // PreviousPositionAtHit = GlobalBlockPosition;

                        }
                        else
                        {

                            PreviousPositionAtHit = GlobalBlockPosition;
                            if (SideDistance.Y < SideDistance.Z)
                            {

                                Distance = SideDistance.Y;
                                SideDistance.Y += DeltaDistance.Y;
                                // PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.Y += Step.Y;

                            }
                            else
                            {

                                Distance = SideDistance.Z;
                                SideDistance.Z += DeltaDistance.Z;
                                // PreviousPositionAtHit = GlobalBlockPosition;
                                GlobalBlockPosition.Z += Step.Z;

                            }
                            //PreviousPositionAtHit = GlobalBlockPosition;

                        }
                        // PreviousPositionAtHit = GlobalBlockPosition;

                    }

                }

            } else
            {

                PreviousPositionAtHit = GlobalBlockPosition;
                if (SideDistance.X < SideDistance.Y)
                {
                    // PreviousPositionAtHit = GlobalBlockPosition;
                    if (SideDistance.X < SideDistance.Z)
                    {

                        Distance = SideDistance.X;
                        SideDistance.X += DeltaDistance.X;
                        // PreviousPositionAtHit = GlobalBlockPosition;
                        GlobalBlockPosition.X += Step.X;

                    }
                    else
                    {

                        Distance = SideDistance.Z;
                        SideDistance.Z += DeltaDistance.Z;
                        // PreviousPositionAtHit = GlobalBlockPosition;
                        GlobalBlockPosition.Z += Step.Z;

                    }
                    // PreviousPositionAtHit = GlobalBlockPosition;

                }
                else
                {

                    PreviousPositionAtHit = GlobalBlockPosition;
                    if (SideDistance.Y < SideDistance.Z)
                    {

                        Distance = SideDistance.Y;
                        SideDistance.Y += DeltaDistance.Y;
                        // PreviousPositionAtHit = GlobalBlockPosition;
                        GlobalBlockPosition.Y += Step.Y;

                    }
                    else
                    {

                        Distance = SideDistance.Z;
                        SideDistance.Z += DeltaDistance.Z;
                        // PreviousPositionAtHit = GlobalBlockPosition;
                        GlobalBlockPosition.Z += Step.Z;

                    }
                    //PreviousPositionAtHit = GlobalBlockPosition;

                }

            }

            */
            // Console.Log("end");

        }

        public static void TraceChunksWhile(Dictionary<Vector3i, PackedChunk> chunkDictionary, Vector3 position, Vector3 direction, int maxSteps)
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

            // Console.Log("start");

            int manhattanDistance = Maths.ManhattanDistance3D(position, GlobalBlockPosition);
            
            bool hit = false;

            // Console.Log(manhattanDistance);

            // if (ChunkLoader.ContainsGeneratedChunk(ChunkUtils.PositionToChunk(GlobalBlockPosition)))
            {

                while (position.X < maxSteps && !hit)
                {

                    // if (ChunkLoader.GetChunk(ChunkUtils.PositionToChunk(GlobalBlockPosition)).GetBlock(ChunkUtils.PositionToBlockLocal(GlobalBlockPosition)) != Blocks.AirBlock)
                    {

                        // ChunkAtHit = (Vector3i)ChunkUtils.PositionToChunk(RoundedPosition);
                        // PositionAtHit = RoundedPosition;
                        ChunkAtHit = (Vector3i)ChunkUtils.PositionToChunk(GlobalBlockPosition);
                        PositionAtHit = GlobalBlockPosition;
                        hit = true;
                        SmoothPosition = position + NormalizedDirection * Distance;

                    }
                    // else
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

            


            // Console.Log("end");

        }

    }
}
