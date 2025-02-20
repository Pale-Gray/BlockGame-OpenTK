using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Worlds;

public class WorldGeneratorUtilities
{
    public static List<Vector3i> GetColumnRing(int radius, int height, Vector3i offset)
    {

        List<Vector3i> positions = new();

        foreach (Vector3i pos in GetRing(radius))
        {

            for (int y = 0; y <= height; y++) 
            {
                positions.Add((pos.X, y, pos.Z) + offset);
            }
            
        }

        return positions.Distinct().ToList();

    }

    private static List<Vector3i> GetRing(int radius)
    {
        
        List<Vector3i> positions = new();

        for (int x = -radius; x <= radius; x++)
        {

            positions.Add((x, 0, radius));
            positions.Add((x, 0, -radius));

            if (x == -radius || x == radius)
            {
                for (int z = -radius + 1; z <= radius - 1; z++)
                {
                    positions.Add((x, 0, z));
                }
            }

        }

        return positions;
        
    }
    
}