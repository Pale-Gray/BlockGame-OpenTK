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
    internal class BlueLightBlock : Block
    {

        public BlueLightBlock()
        {

            SetProperties(LoadFromJson("BlueLightBlock.json"));

        }

        public override void OnBlockSet(Chunk chunk, Vector3i localBlockPosition)
        {

            chunk.SetBlockLight(localBlockPosition, (0, 0, 15));
            base.OnBlockSet(chunk, localBlockPosition);

        }

        public override void OnBlockPlace(World world, BlockProperties blockProperties, Vector3i globalBlockPosition)
        {

            world.AddBlockLight(globalBlockPosition, (0, 0, 15));
            base.OnBlockPlace(world, blockProperties, globalBlockPosition);

        }

        public override void OnBlockDestroy(World world, Vector3i globalBlockPosition)
        {

            world.RemoveBlockLight(globalBlockPosition);
            base.OnBlockDestroy(world, globalBlockPosition);

        }

    }
}
