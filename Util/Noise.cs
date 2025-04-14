using System;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using Game.Util;
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

}