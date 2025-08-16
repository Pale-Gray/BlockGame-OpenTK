using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace VoxelGame;

public class Noise
{
    public static uint Random2(int seed, Vector2i position)
    {
        uint s = Unsafe.BitCast<int, uint>(seed);
        uint x = Unsafe.BitCast<int, uint>(position.X);
        uint y = Unsafe.BitCast<int, uint>(position.Y);

        s ^= s << 17;
        s ^= s << 7;
        s ^= s << 11;
    
        x ^= x << 15;
        x ^= x << 7;
        x ^= x << 9;
    
        y ^= y << 15;
        y ^= y << 7;
        y ^= y << 11;

        uint result = s + y + x;
        result ^= result << 9;
        result ^= result << 7;
        result ^= result << 3;
        
        return result;
    }

    public static float FloatRandom2(int seed, Vector2i position) => Random2(seed, position) / (float)uint.MaxValue;

    // as defined in https://registry.khronos.org/OpenGL-Refpages/gl4/html/smoothstep.xhtml, except it's expected t is from 0 to 1
    public static float Smoothstep(float t) => t * t * (3.0f - 2.0f * t);
    
    public static float ValueNoise2(int seed, Vector2 position)
    {
        Vector2 interpolant = (Maths.EuclideanRemainder(position.X, 1), Maths.EuclideanRemainder(position.Y, 1));
        Vector2i samplePosition = new Vector2i((int)float.Floor(position.X), (int)float.Floor(position.Y));

        float bottomLeft = FloatRandom2(seed, samplePosition);
        float bottomRight = FloatRandom2(seed, samplePosition + Vector2i.UnitX);
        float topLeft = FloatRandom2(seed, samplePosition + Vector2i.UnitY);
        float topRight = FloatRandom2(seed, samplePosition + Vector2i.One);

        float top = float.Lerp(topLeft, topRight, Smoothstep(interpolant.X));
        float bottom = float.Lerp(bottomLeft, bottomRight, Smoothstep(interpolant.X));
        
        return float.Lerp(bottom, top, Smoothstep(interpolant.Y));
    }
}