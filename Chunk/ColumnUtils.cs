using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Game.Core.Worlds;
using Game.Util;
using OpenTK.Mathematics;

namespace Game.Core.Chunks;

public class ColumnUtils
{

    public static Vector4 GetNormalizedLightValues(ChunkColumn column, Vector3i localPosition)
    {

        Vector3i chunkPosition = ChunkUtils.PositionToChunk(localPosition);
        Vector3i chunkLocalPosition = ChunkUtils.PositionToBlockLocal(localPosition);

        if (chunkPosition.Y < 0) return Vector4.Zero;
        if (chunkPosition.Y >= WorldGenerator.WorldGenerationHeight) return Vector4.One;

        ushort lightData = column.Chunks[chunkPosition.Y].LightData[ChunkUtils.VecToIndex(chunkLocalPosition)];

        Vector4 lightValue = Vector4.Zero;

        lightValue.X = ((lightData >> 12) & 15) / 15.0f;
        lightValue.Y = ((lightData >> 8) & 15) / 15.0f;
        lightValue.Z = ((lightData >> 4) & 15) / 15.0f;
        lightValue.W = (lightData & 15) / 15.0f;

        return lightValue;

    }

    public static void SetBlueBlocklightValue(ChunkColumn column, Vector3i localPosition, ushort value)
    {

        Vector3i chunkPosition = ChunkUtils.PositionToChunk(localPosition);
        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(localPosition);

        column.Chunks[chunkPosition.Y].LightData[ChunkUtils.VecToIndex(localBlockPosition)] = (ushort) ((column.Chunks[chunkPosition.Y].LightData[ChunkUtils.VecToIndex(localBlockPosition)] & ~(15 << 4)) | (value << 4));

    }
    public static ushort GetBlueBlocklightValue(ChunkColumn column, Vector3i localPosition)
    {

        Vector3i chunkPosition = ChunkUtils.PositionToChunk(localPosition);
        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(localPosition);

        return (ushort) ((column.Chunks[chunkPosition.Y].LightData[ChunkUtils.VecToIndex(localBlockPosition)] >> 4) & 15);

    }

    public static void SetGreenBlocklightValue(ChunkColumn column, Vector3i localPosition, ushort value)
    {

        Vector3i chunkPosition = ChunkUtils.PositionToChunk(localPosition);
        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(localPosition);

        column.Chunks[chunkPosition.Y].LightData[ChunkUtils.VecToIndex(localBlockPosition)] = (ushort) ((column.Chunks[chunkPosition.Y].LightData[ChunkUtils.VecToIndex(localBlockPosition)] & ~(15 << 8)) | (value << 8));

    }
    public static ushort GetGreenBlocklightValue(ChunkColumn column, Vector3i localPosition)
    {

        Vector3i chunkPosition = ChunkUtils.PositionToChunk(localPosition);
        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(localPosition);

        return (ushort) ((column.Chunks[chunkPosition.Y].LightData[ChunkUtils.VecToIndex(localBlockPosition)] >> 8) & 15);

    }
    public static void SetRedBlocklightValue(ChunkColumn column, Vector3i localPosition, ushort value)
    {

        Vector3i chunkPosition = ChunkUtils.PositionToChunk(localPosition);
        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(localPosition);

        column.Chunks[chunkPosition.Y].LightData[ChunkUtils.VecToIndex(localBlockPosition)] = (ushort) ((column.Chunks[chunkPosition.Y].LightData[ChunkUtils.VecToIndex(localBlockPosition)] & ~(15 << 12)) | (value << 12));

    }
    public static ushort GetRedBlocklightValue(ChunkColumn column, Vector3i localPosition)
    {

        Vector3i chunkPosition = ChunkUtils.PositionToChunk(localPosition);
        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(localPosition);

        return (ushort) ((column.Chunks[chunkPosition.Y].LightData[ChunkUtils.VecToIndex(localBlockPosition)] >> 12) & 15);

    }
    public static void SetSunlightValue(ChunkColumn column, Vector3i localPosition, ushort value)
    {

        Vector3i chunkPosition = ChunkUtils.PositionToChunk(localPosition);
        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(localPosition);

        column.Chunks[chunkPosition.Y].LightData[ChunkUtils.VecToIndex(localBlockPosition)] = (ushort) ((column.Chunks[chunkPosition.Y].LightData[ChunkUtils.VecToIndex(localBlockPosition)] & ~15) | value);

    }

    public static ushort GetSunlightValue(ChunkColumn column, Vector3i localPosition)
    {

        Vector3i chunkPosition = ChunkUtils.PositionToChunk(localPosition);
        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(localPosition);

        return (ushort) (column.Chunks[chunkPosition.Y].LightData[ChunkUtils.VecToIndex(localBlockPosition)] & 15);

    }

    public static void SetHeightmap(ChunkColumn column, Vector3i globalPosition)
    {

        if (globalPosition.Y >= GlobalValues.ChunkSize * WorldGenerator.WorldGenerationHeight || globalPosition.Y < 0) return;

        int height = column.SolidHeightmap[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalPosition).Xz)];
        column.SolidHeightmap[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalPosition).Xz)] = Math.Max(height, globalPosition.Y);

    }

    public static int GetHeightmap(ChunkColumn column, Vector2i localPosition) => column.SolidHeightmap[ChunkUtils.VecToIndex(localPosition)];

    public static Vector3i GlobalToLocal(Vector3i globalPosition)
    {

        globalPosition.X = Maths.Mod(globalPosition.X, GlobalValues.ChunkSize);
        // globalPosition.Y = Maths.Mod(globalPosition.Y, GlobalValues.ChunkSize * GameState.WorldGenerationHeight);
        globalPosition.Z = Maths.Mod(globalPosition.Z, GlobalValues.ChunkSize);

        return globalPosition;

    }

    public static Vector2i PositionToChunk(Vector3i globalPosition)
    {

        Vector2i pos = globalPosition.Xz;

        pos.X = (int) Math.Floor(globalPosition.X / (float) GlobalValues.ChunkSize);
        pos.Y = (int) Math.Floor(globalPosition.Z / (float) GlobalValues.ChunkSize);

        return pos; 

    }
    public static void SetSolidBlock(ChunkColumn column, Vector3i globalBlockPosition, bool isSolid)
    {

        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);
        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);

        column.Chunks[chunkPosition.Y].SolidMask[ChunkUtils.VecToIndex(localBlockPosition.Xz)] &= ~(1u << localBlockPosition.Y);
        column.Chunks[chunkPosition.Y].SolidMask[ChunkUtils.VecToIndex(localBlockPosition.Xz)] |= (isSolid ? 1u : 0u) << localBlockPosition.Y;

    }

    public static bool GetSolidBlock(ChunkColumn column, Vector3i globalBlockPosition)
    {

        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);
        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);

        if (chunkPosition.Y < 0 || chunkPosition.Y >= WorldGenerator.WorldGenerationHeight) return false;

        return (column.Chunks[chunkPosition.Y].SolidMask[ChunkUtils.VecToIndex(localBlockPosition.Xz)] & (1u << localBlockPosition.Y)) != 0u;

    }
    public static void SetBlockId(ChunkColumn column, Vector3i globalBlockPosition, ushort id)
    {

        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);
        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);

        column.Chunks[chunkPosition.Y].BlockData[ChunkUtils.VecToIndex(localBlockPosition)] = id;

    }

    public static ushort GetBlockId(ChunkColumn column, Vector3i globalBlockPosition)
    {

        Vector3i localBlockPosition = ChunkUtils.PositionToBlockLocal(globalBlockPosition);
        Vector3i chunkPosition = ChunkUtils.PositionToChunk(globalBlockPosition);

        return column.Chunks[chunkPosition.Y].BlockData[ChunkUtils.VecToIndex(localBlockPosition)];

    }

    public static string PositionToFilename(Vector2i columnPosition) => $"{columnPosition.X}_{columnPosition.Y}.cdat";
    public static Vector2i FilenameToPosition(string fileName)
    {

        string[] pos = fileName.Split('_');
        pos[1] = pos[1].Split('.')[0];

        return (int.Parse(pos[0]), int.Parse(pos[1]));

    }

}