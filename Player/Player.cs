using System.Collections.Generic;
using OpenTK.Mathematics;

namespace VoxelGame;

public class Player
{
    public string Name;
    public Vector3 Position;
    public HashSet<Vector2i> LoadedChunks = new();

    public Player(string name)
    {
        Name = name;
    }
}