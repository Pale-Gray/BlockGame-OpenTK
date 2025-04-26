using System;
using System.Threading;
using Game.Core.Worlds;
using Game.Util;
using OpenTK.Mathematics;

namespace Game.Core.Generation;

public class RedMushroomBiome : Biome
{

    public override void OnTerrainPass(World world, Vector3i globalBlockPosition)
    {

        if (Noise.FloatRandom2(0, globalBlockPosition.Xz) > 0.9f && !world.GetSolidBlock(globalBlockPosition) && world.GetSolidBlock(globalBlockPosition - Vector3i.UnitY))
        {

            GlobalValues.Register.GetBlockFromNamespace("Game.TinyRedMushroom").OnBlockPlace(world, globalBlockPosition, false, false);

        }

        if (!world.GetSolidBlock(globalBlockPosition) && world.GetSolidBlock(globalBlockPosition - Vector3i.UnitY))
        {

            for (int i = 0; i < 5; i++)
            {

                if (i == 0)
                {

                    GlobalValues.Register.GetBlockFromNamespace("Game.GrassBlock").OnBlockPlace(world, globalBlockPosition - (0, 1 + i, 0), false, false);

                } else
                {

                    if (world.GetSolidBlock(globalBlockPosition - (0, 1 + i, 0))) GlobalValues.Register.GetBlockFromNamespace("Game.DirtBlock").OnBlockPlace(world, globalBlockPosition - (0, 1 + i, 0), false, false);

                }

            }

        }

    }

}