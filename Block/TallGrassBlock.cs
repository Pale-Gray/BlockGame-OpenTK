using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.BlockUtil
{
    internal class TallGrassBlock : Block
    {

        private Block _regularTallGrass = LoadFromJson("TallGrassBlock.json");
        public TallGrassBlock()
        {

            SetProperties(_regularTallGrass);

        }

        public override void OnBlockSet(Chunk chunk, Vector3i localBlockPosition)
        {

            chunk.SetBlockLight(localBlockPosition, (7,7,7));
            base.OnBlockSet(chunk, localBlockPosition);

        }

        public override void OnBlockPlace(World world, Vector3i globalBlockPosition)
        {

            world.AddBlockLight(globalBlockPosition, (7, 7, 7));
            base.OnBlockPlace(world, globalBlockPosition);

        }

        public override void OnBlockDestroy(World world, Vector3i globalBlockPosition)
        {

            world.RemoveBlockLight(globalBlockPosition);
            base.OnBlockDestroy(world, globalBlockPosition);

        }

        public override void OnBlockMesh(World world, Dictionary<Vector3i, bool[]> mask, BlockProperty.BlockProperties blockProperties, Vector3i globalBlockPosition)
        {

            base.OnBlockMesh(world, mask, blockProperties, globalBlockPosition);

        }

    }
}
