using System.Collections.Generic;
using System.Net.Http;
using Blockgame_OpenTK.PlayerUtil;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Networking;

public class PlayerChunkArea
{
    public HashSet<Vector2i> CurrentChunkArea = new();

    public void ComputeDiffs(Server server, Vector2i PlayerChunkPosition)
    {

        HashSet<Vector2i> newHashSet = new();
        foreach (Vector2i pos in CurrentChunkArea)
        {
            newHashSet.Add(pos + PlayerChunkPosition);
        }

        foreach (Vector2i pos in CurrentChunkArea)
        {
            if (!newHashSet.Contains(pos))
            {
                // save, then remove on client!
                // compare against all player position areas, if they all dont exist, remove it on the server too

                foreach (PlayerChunkArea area in server.PlayerChunkAreas.Values)
                {
                    if (!area.CurrentChunkArea.Contains(pos))
                    {
                        // remove from the server, its fine!
                    }

                }

            }
        }

        foreach (Vector2i pos in newHashSet)
        {
            if (!CurrentChunkArea.Contains(pos))
            {
                // add new chunks! send them to client!
            }
        }

        CurrentChunkArea = newHashSet;

    }

}