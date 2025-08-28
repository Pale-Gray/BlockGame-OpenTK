using OpenTK.Mathematics;

namespace VoxelGame;

public struct BoundingBox
{
    public Vector3 Size = Vector3.One;
    public Vector3 Position = Vector3.Zero;
    public Vector3 Center => Position + (Size / 2.0f);
    public Vector3 Min => Position;
    public Vector3 Max => Position + Size;
    
    public float Bounciness = 0.0f;
    public float Friction = 1.0f;
    
    public BoundingBox()
    {
        
    }

    public BoundingBox(Vector3 size)
    {
        Size = size;
    }

    public BoundingBox Offset(Vector3 position)
    {
        Position = position;
        return this;
    }

    public bool CollidesWith(BoundingBox box)
    {
        return false;
    }
}