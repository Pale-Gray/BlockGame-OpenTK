using OpenTK.Mathematics;

namespace VoxelGame;

public struct BoundingBox
{
    public Vector3 Size = Vector3.One;
    public Vector3 Position = Vector3.Zero;
    public float Bounciness = 0.0f;
    public float Friction = 1.0f;
    
    public BoundingBox()
    {
        
    }
}