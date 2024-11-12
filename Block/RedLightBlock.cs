using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.BlockUtil
{
    internal class RedLightBlock : Block
    {

        public RedLightBlock()
        {

            SetProperties(LoadFromJson("RedLightBlock.json"));

        }

        public override void OnBlockSet(Chunk chunk, Vector3i localBlockPosition)
        {

            chunk.SetBlockLight(localBlockPosition, (15, 0, 0));
            base.OnBlockSet(chunk, localBlockPosition);

        }

        public override void OnBlockPlace(World world, BlockProperties blockProperties, Vector3i globalBlockPosition)
        {

            world.AddBlockLight(globalBlockPosition, (15, 0, 0));
            base.OnBlockPlace(world, blockProperties, globalBlockPosition);

        }

        public override void OnBlockDestroy(World world, Vector3i globalBlockPosition)
        {

            world.RemoveBlockLight(globalBlockPosition);
            base.OnBlockDestroy(world, globalBlockPosition);

        }

    }
}
