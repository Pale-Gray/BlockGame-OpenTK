using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Chunks
{
    internal class ChunkMeshHandler
    {

        public static void AddModel(World world, Vector3i globalBlockPosition, BlockModel blockModel)
        {

            if (blockModel != null)
            {

                // if (blockModel.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.None)) world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].AddBlockModelFace(blockModel.GetFaceWithOffsetAO(world.WorldChunks, ChunkUtils.PositionToChunk(globalBlockPosition), BlockModelCullDirection.None, ChunkUtils.PositionToBlockLocal(globalBlockPosition)));

                if ((!ChunkUtils.GetSolidBlock(world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (0, 1, 0))], ChunkUtils.PositionToBlockLocal(globalBlockPosition + (0, 1, 0)))
                 || (!world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (0, 1, 0))].GetBlock(ChunkUtils.PositionToBlockLocal(globalBlockPosition + (0,1,0))).IsCullable ?? false))
                 && blockModel.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Up))
                {
                    
                    // world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].AddBlockModelFace(blockModel.GetFaceWithOffsetAO(world.WorldChunks, ChunkUtils.PositionToChunk(globalBlockPosition), BlockModelCullDirection.Up, ChunkUtils.PositionToBlockLocal(globalBlockPosition)));

                }
                if ((!ChunkUtils.GetSolidBlock(world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (0, -1, 0))], ChunkUtils.PositionToBlockLocal(globalBlockPosition + (0, -1, 0)))
                 || (!world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (0, -1, 0))].GetBlock(ChunkUtils.PositionToBlockLocal(globalBlockPosition + (0, -1, 0))).IsCullable ?? false))
                 && blockModel.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Down))
                {

                    // world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].AddBlockModelFace(blockModel.GetFaceWithOffsetAO(world.WorldChunks, ChunkUtils.PositionToChunk(globalBlockPosition), BlockModelCullDirection.Down, ChunkUtils.PositionToBlockLocal(globalBlockPosition)));

                }
                if ((!ChunkUtils.GetSolidBlock(world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (1, 0, 0))], ChunkUtils.PositionToBlockLocal(globalBlockPosition + (1, 0, 0)))
                 || (!world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (1, 0, 0))].GetBlock(ChunkUtils.PositionToBlockLocal(globalBlockPosition + (1, 0, 0))).IsCullable ?? false))
                 && blockModel.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Left))
                {

                    // world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].AddBlockModelFace(blockModel.GetFaceWithOffsetAO(world.WorldChunks, ChunkUtils.PositionToChunk(globalBlockPosition), BlockModelCullDirection.Left, ChunkUtils.PositionToBlockLocal(globalBlockPosition)));

                }
                if ((!ChunkUtils.GetSolidBlock(world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (-1, 0, 0))], ChunkUtils.PositionToBlockLocal(globalBlockPosition + (-1, 0, 0)))
                 || (!world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (-1, 0, 0))].GetBlock(ChunkUtils.PositionToBlockLocal(globalBlockPosition + (-1, 0, 0))).IsCullable ?? false))
                 && blockModel.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Right))
                {

                    // world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].AddBlockModelFace(blockModel.GetFaceWithOffsetAO(world.WorldChunks, ChunkUtils.PositionToChunk(globalBlockPosition), BlockModelCullDirection.Right, ChunkUtils.PositionToBlockLocal(globalBlockPosition)));

                }
                if ((!ChunkUtils.GetSolidBlock(world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (0, 0, 1))], ChunkUtils.PositionToBlockLocal(globalBlockPosition + (0, 0, 1)))
                 || (!world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (0, 0, 1))].GetBlock(ChunkUtils.PositionToBlockLocal(globalBlockPosition + (0, 0, 1))).IsCullable ?? false))
                 && blockModel.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Back))
                {

                    // world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].AddBlockModelFace(blockModel.GetFaceWithOffsetAO(world.WorldChunks, ChunkUtils.PositionToChunk(globalBlockPosition), BlockModelCullDirection.Back, ChunkUtils.PositionToBlockLocal(globalBlockPosition)));

                }
                if ((!ChunkUtils.GetSolidBlock(world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (0, 0, -1))], ChunkUtils.PositionToBlockLocal(globalBlockPosition + (0, 0, -1)))
                 || (!world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (0, 0, -1))].GetBlock(ChunkUtils.PositionToBlockLocal(globalBlockPosition + (0, 0, -1))).IsCullable ?? false))
                 && blockModel.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Front))
                {

                    // world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].AddBlockModelFace(blockModel.GetFaceWithOffsetAO(world.WorldChunks, ChunkUtils.PositionToChunk(globalBlockPosition), BlockModelCullDirection.Front, ChunkUtils.PositionToBlockLocal(globalBlockPosition)));

                }

            } else
            {

                AddModel(world, globalBlockPosition, GlobalValues.MissingBlockModel);

            }

        }

    }
}
