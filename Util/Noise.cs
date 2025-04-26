using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading;
using Game.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace Game.Core.Generation;

public class Noise
{

    public static int Random2(int seed, Vector2i coordinates)
    {

        uint s = Unsafe.BitCast<int, uint>(seed);
        uint x = Unsafe.BitCast<int, uint>(coordinates.X);
        uint y = Unsafe.BitCast<int, uint>(coordinates.Y);

        s ^= s << 60;
        s ^= s << 10;
        s ^= s << 7;
        
        x ^= x << 40;
        x ^= x << 10;
        x ^= x << 7;
    
        y ^= y << 50;
        y ^= y << 15;
        y ^= y << 4;
        
        uint value = s+y+x;
        
        value ^= value << 60;
        value ^= value << 10;
        value ^= value << 3;
        
        return unchecked(Unsafe.BitCast<uint, int>(value));

    }

    public static int Random3(int seed, Vector3i coordinates)
    {

        uint s = Unsafe.BitCast<int, uint>(seed);
        uint x = Unsafe.BitCast<int, uint>(coordinates.X);
        uint y = Unsafe.BitCast<int, uint>(coordinates.Y);
        uint z = Unsafe.BitCast<int, uint>(coordinates.Z);

        s ^= s << 60;
        s ^= s << 10;
        s ^= s << 7;
        
        x ^= x << 40;
        x ^= x << 10;
        x ^= x << 7;
    
        y ^= y << 50;
        y ^= y << 15;
        y ^= y << 4;

        z ^= z << 60;
        z ^= z << 13;
        z ^= z << 6;
        
        uint value = s+y+x+z;
        
        value ^= value << 60;
        value ^= value << 10;
        value ^= value << 3;
        
        return unchecked(Unsafe.BitCast<uint, int>(value));

    }

    public static float FloatRandom2(int seed, Vector2i coordinates)
    {

        return Random2(seed, coordinates) / (float) int.MaxValue;

    }

    public static float FloatRandom3(int seed, Vector3i coordinates)
    {

        return Random3(seed, coordinates) / (float) int.MaxValue;

    }

    public static int IntRandom2(int seed, Vector2i coordinates, int lowerBound, int upperBound)
    {

        float value = FloatRandom2(seed, coordinates);
        value = (value + 1.0f) / 2.0f;

        return (int) Math.Floor(Maths.Lerp(lowerBound, upperBound, value));

    }

    public static int IntRandom3(int seed, Vector3i coordinates, int lowerBound, int upperBound)
    {

        float value = FloatRandom3(seed, coordinates);
        value = (value + 1.0f) / 2.0f;

        return (int) Math.Floor(Maths.Lerp(lowerBound, upperBound, value));

    }

    private static float FlooredRemainder(float value, float divisor)
    {

        return value - divisor * (float) Math.Floor(value / divisor);

    }

    // https://en.wikipedia.org/wiki/Smoothstep
    private static float Smoothstep(float a, float b, float t)
    {

        float v = Math.Clamp((t - a) / (b - a), 0.0f, 1.0f);
        return v * v * (3.0f - 2.0f * v);

    }

    public static float Value2(int seed, Vector2 coordinates)
    {

        Vector2i topLeft = ((int)Math.Floor(coordinates.X), (int)Math.Floor(coordinates.Y) + 1);
        Vector2i topRight = ((int)Math.Floor(coordinates.X) + 1, (int)Math.Floor(coordinates.Y) + 1);
        Vector2i bottomLeft = ((int)Math.Floor(coordinates.X), (int)Math.Floor(coordinates.Y));
        Vector2i bottomRight = ((int)Math.Floor(coordinates.X) + 1, (int)Math.Floor(coordinates.Y));

        float valueTopLeft = FloatRandom2(seed, topLeft);
        float valueTopRight = FloatRandom2(seed, topRight);
        float valueBottomLeft = FloatRandom2(seed, bottomLeft);
        float valueBottomRight = FloatRandom2(seed, bottomRight);

        float xInterp = FlooredRemainder(coordinates.X, 1.0f);
        float yInterp = FlooredRemainder(coordinates.Y, 1.0f);

        float top = float.Lerp(valueTopLeft, valueTopRight, Smoothstep(0.0f, 1.0f, xInterp));
        float bottom = float.Lerp(valueBottomLeft, valueBottomRight, Smoothstep(0.0f, 1.0f, xInterp));

        return (float.Lerp(bottom, top, Smoothstep(0.0f, 1.0f, yInterp)) + 1.0f) / 2.0f;

    }

    public static float OctaveValue2(int seed, Vector2 coordinates, int octaves)
    {

        float value = 0.0f;

        for (float o = 1; o <= octaves; o++)
        {

            value += Value2(seed, coordinates * o) * (octaves / (octaves + o));

        }

        return value / octaves;

    }

    public static float Value3(int seed, Vector3 coordinates)
    {

        Vector3i upTopLeft = ((int)Math.Floor(coordinates.X), (int)Math.Floor(coordinates.Y) + 1, (int)Math.Floor(coordinates.Z) + 1);
        Vector3i upTopRight = ((int)Math.Floor(coordinates.X) + 1, (int)Math.Floor(coordinates.Y) + 1, (int)Math.Floor(coordinates.Z) + 1);
        Vector3i upBottomLeft = ((int)Math.Floor(coordinates.X), (int)Math.Floor(coordinates.Y), (int)Math.Floor(coordinates.Z) + 1);
        Vector3i upBottomRight = ((int)Math.Floor(coordinates.X) + 1, (int)Math.Floor(coordinates.Y), (int)Math.Floor(coordinates.Z) + 1);

        Vector3i downTopLeft = ((int)Math.Floor(coordinates.X), (int)Math.Floor(coordinates.Y) + 1, (int)Math.Floor(coordinates.Z));
        Vector3i downTopRight = ((int)Math.Floor(coordinates.X) + 1, (int)Math.Floor(coordinates.Y) + 1, (int)Math.Floor(coordinates.Z));
        Vector3i downBottomLeft = ((int)Math.Floor(coordinates.X), (int)Math.Floor(coordinates.Y), (int)Math.Floor(coordinates.Z));
        Vector3i downBottomRight = ((int)Math.Floor(coordinates.X) + 1, (int)Math.Floor(coordinates.Y), (int)Math.Floor(coordinates.Z));

        float valueUpTopLeft = FloatRandom3(seed, upTopLeft);
        float valueUpTopRight = FloatRandom3(seed, upTopRight);
        float valueUpBottomLeft = FloatRandom3(seed, upBottomLeft);
        float valueUpBottomRight = FloatRandom3(seed, upBottomRight);

        float valueDownTopLeft = FloatRandom3(seed, downTopLeft);
        float valueDownTopRight = FloatRandom3(seed, downTopRight);
        float valueDownBottomLeft = FloatRandom3(seed, downBottomLeft);
        float valueDownBottomRight = FloatRandom3(seed, downBottomRight);

        float xInterp = FlooredRemainder(coordinates.X, 1.0f);
        float yInterp = FlooredRemainder(coordinates.Y, 1.0f);
        float zInterp = FlooredRemainder(coordinates.Z, 1.0f);

        float upTop = float.Lerp(valueUpTopLeft, valueUpTopRight, Smoothstep(0.0f, 1.0f, xInterp));
        float upBottom = float.Lerp(valueUpBottomLeft, valueUpBottomRight, Smoothstep(0.0f, 1.0f, xInterp));

        float up = float.Lerp(upBottom, upTop, Smoothstep(0.0f, 1.0f, yInterp));

        float downTop = float.Lerp(valueDownTopLeft, valueDownTopRight, Smoothstep(0.0f, 1.0f, xInterp));
        float downBottom = float.Lerp(valueDownBottomLeft, valueDownBottomRight, Smoothstep(0.0f, 1.0f, xInterp));

        float down = float.Lerp(downBottom, downTop, Smoothstep(0.0f, 1.0f, yInterp));

        return (float.Lerp(down, up, Smoothstep(0.0f, 1.0f, zInterp)) + 1.0f) / 2.0f;

    }

    private static ConcurrentDictionary<(int seed, Vector3i coordinate), float> _cachedValues = new();

    public static float UpsampledValue3(int seed, Vector3 coordinates, Vector3i cellSize)
    {

        Vector3i upTopLeft = ((int)Math.Floor(coordinates.X / cellSize.X), (int)Math.Floor(coordinates.Y / cellSize.Y) + cellSize.Y, (int)Math.Floor(coordinates.Z / cellSize.Z) + cellSize.Z);
        Vector3i upTopRight = ((int)Math.Floor(coordinates.X / cellSize.X) + cellSize.X, (int)Math.Floor(coordinates.Y / cellSize.Y) + cellSize.Y, (int)Math.Floor(coordinates.Z / cellSize.Z) + cellSize.Z);
        Vector3i upBottomLeft = ((int)Math.Floor(coordinates.X / cellSize.X), (int)Math.Floor(coordinates.Y / cellSize.Y), (int)Math.Floor(coordinates.Z / cellSize.Z) + cellSize.Z);
        Vector3i upBottomRight = ((int)Math.Floor(coordinates.X / cellSize.X) + cellSize.X, (int)Math.Floor(coordinates.Y / cellSize.Y), (int)Math.Floor(coordinates.Z / cellSize.Z) + cellSize.Z);

        Vector3i downTopLeft = ((int)Math.Floor(coordinates.X / cellSize.X), (int)Math.Floor(coordinates.Y / cellSize.Y) + cellSize.Y, (int)Math.Floor(coordinates.Z / cellSize.Z));
        Vector3i downTopRight = ((int)Math.Floor(coordinates.X / cellSize.X) + cellSize.X, (int)Math.Floor(coordinates.Y / cellSize.Y) + cellSize.Y, (int)Math.Floor(coordinates.Z / cellSize.Z));
        Vector3i downBottomLeft = ((int)Math.Floor(coordinates.X / cellSize.X), (int)Math.Floor(coordinates.Y / cellSize.Y), (int)Math.Floor(coordinates.Z / cellSize.Z));
        Vector3i downBottomRight = ((int)Math.Floor(coordinates.X / cellSize.X) + cellSize.X, (int)Math.Floor(coordinates.Y / cellSize.Y), (int)Math.Floor(coordinates.Z / cellSize.Z));

        float valueUpTopLeft = 0.0f;
        if (!_cachedValues.TryGetValue((seed, upTopLeft), out valueUpTopLeft))
        {
            valueUpTopLeft = Value3(seed, upTopLeft);
            _cachedValues.TryAdd((seed, upTopLeft), valueUpTopLeft);
        }
        float valueUpTopRight = 0.0f;
        if (!_cachedValues.TryGetValue((seed, upTopRight), out valueUpTopRight))
        {
            valueUpTopRight = Value3(seed, upTopRight);
            _cachedValues.TryAdd((seed, upTopRight), valueUpTopRight);
        }
        float valueUpBottomLeft = 0.0f;
        if (!_cachedValues.TryGetValue((seed, upBottomLeft), out valueUpBottomLeft))
        {
            valueUpBottomLeft = Value3(seed, upBottomLeft);
            _cachedValues.TryAdd((seed, upBottomLeft), valueUpBottomLeft);
        }
        float valueUpBottomRight = 0.0f;
        if (!_cachedValues.TryGetValue((seed, upBottomRight), out valueUpBottomRight))
        {
            valueUpBottomRight = Value3(seed, upBottomRight);
            _cachedValues.TryAdd((seed, upBottomRight), valueUpBottomRight);
        }

        float valueDownTopLeft = 0.0f;
        if (!_cachedValues.TryGetValue((seed, downTopLeft), out valueDownTopLeft))
        {
            valueDownTopLeft = Value3(seed, downTopLeft);
            _cachedValues.TryAdd((seed, downTopLeft), valueDownTopLeft);
        }
        float valueDownTopRight = 0.0f;
        if (!_cachedValues.TryGetValue((seed, downTopRight), out valueDownTopRight))
        {
            valueDownTopRight = Value3(seed, downTopRight);
            _cachedValues.TryAdd((seed, downTopRight), valueDownTopRight);
        }
        float valueDownBottomLeft = 0.0f;
        if (!_cachedValues.TryGetValue((seed, downBottomLeft), out valueDownBottomLeft))
        {
            valueDownBottomLeft = Value3(seed, downBottomLeft);
            _cachedValues.TryAdd((seed, downBottomLeft), valueDownBottomLeft);
        }
        float valueDownBottomRight = 0.0f;
        if (!_cachedValues.TryGetValue((seed, downBottomRight), out valueDownBottomRight))
        {
            valueDownBottomRight = Value3(seed, downBottomRight);
            _cachedValues.TryAdd((seed, downBottomRight), valueDownBottomRight);
        }

        float xInterp = FlooredRemainder(coordinates.X / cellSize.X, 1.0f);
        float yInterp = FlooredRemainder(coordinates.Y / cellSize.Y, 1.0f);
        float zInterp = FlooredRemainder(coordinates.Z / cellSize.Z, 1.0f);

        float upTop = float.Lerp(valueUpTopLeft, valueUpTopRight, xInterp);
        float upBottom = float.Lerp(valueUpBottomLeft, valueUpBottomRight, xInterp);

        float up = float.Lerp(upBottom, upTop, yInterp);

        float downTop = float.Lerp(valueDownTopLeft, valueDownTopRight, xInterp);
        float downBottom = float.Lerp(valueDownBottomLeft, valueDownBottomRight, xInterp);

        float down = float.Lerp(downBottom, downTop, yInterp);

        return float.Lerp(down, up, zInterp);

    }

    public static float OctaveValue3(int seed, Vector3 coordinates, int octaves)
    {

        float value = 0.0f;

        for (float o = 1; o <= octaves; o++)
        {

            value += Value3(seed, coordinates * o) / (octaves / (octaves + o));

        }

        return value / octaves;

    }

    private static Vector3 FlooredRemainder(Vector3 a, float b)
    {

        return (FlooredRemainder(a.X, b), FlooredRemainder(a.Y, b), FlooredRemainder(a.Z, b));

    } 

    public static float[] CoarseValue3(int seed, Vector3i originalSize, Vector3i factor, Vector3i startingPosition, Vector3 positionFactor)
    {

        Vector3i size = (originalSize / factor) + Vector3i.One;

        float[] values = new float[size.X * size.Y * size.Z];

        for (int x = 0; x < size.X; x++)
        {

            for (int y = 0; y < size.Y; y++)
            {

                for (int z = 0; z < size.Z; z++)
                {

                    values[Maths.VecToIndex(x,y,z,size.X,size.Y)] = Value3(seed, ((Vector3)startingPosition / positionFactor) + ((x,y,z) * ((Vector3)factor / positionFactor)));
                    
                }

            }

        }

        return values;

    }

    public static float InterpolatedValue3(float[] samples, Vector3i originalSize, Vector3i factor, Vector3 localPosition)
    {

        Vector3i size = (originalSize / factor) + Vector3i.One;
        Vector3 interpolant = FlooredRemainder(localPosition / (Vector3) factor, 1.0f);
        Vector3i cell = VectorMath.Floor(localPosition / (Vector3) factor);

        float valueUpTopLeft = samples[Maths.VecToIndex(cell.X, cell.Y + 1, cell.Z + 1, size.X, size.Y)];
        float valueUpTopRight = samples[Maths.VecToIndex(cell.X+1, cell.Y + 1, cell.Z + 1, size.X, size.Y)];
        float valueUpBottomLeft = samples[Maths.VecToIndex(cell.X, cell.Y, cell.Z + 1, size.X, size.Y)];
        float valueUpBottomRight = samples[Maths.VecToIndex(cell.X+1, cell.Y, cell.Z + 1, size.X, size.Y)];

        float valueDownTopLeft = samples[Maths.VecToIndex(cell.X, cell.Y + 1, cell.Z, size.X, size.Y)];
        float valueDownTopRight = samples[Maths.VecToIndex(cell.X+1, cell.Y + 1, cell.Z, size.X, size.Y)];
        float valueDownBottomLeft = samples[Maths.VecToIndex(cell.X, cell.Y, cell.Z, size.X, size.Y)];
        float valueDownBottomRight = samples[Maths.VecToIndex(cell.X+1, cell.Y, cell.Z, size.X, size.Y)];

        float valueUpTop = float.Lerp(valueUpTopLeft, valueUpTopRight, interpolant.X);
        float valueUpBottom = float.Lerp(valueUpBottomLeft, valueUpBottomRight, interpolant.X);

        float valueUp = float.Lerp(valueUpBottom, valueUpTop, interpolant.Y);

        float valueDownTop = float.Lerp(valueDownTopLeft, valueDownTopRight, interpolant.X);
        float valueDownBottom = float.Lerp(valueDownBottomLeft, valueDownBottomRight, interpolant.X);

        float valueDown = float.Lerp(valueDownBottom, valueDownTop, interpolant.Y);

        return float.Lerp(valueDown, valueUp, interpolant.Z);

    }

}