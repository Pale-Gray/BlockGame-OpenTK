using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Core.Worlds;
using OpenTK.Graphics.Vulkan;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Core.Chunks
{
    internal class ChunkMeshHandler
    {

        public static void AddModel(World world, Vector3i globalBlockPosition, BlockModel blockModel)
        {

            if (blockModel.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.None)) world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].OpaqueMeshList.AddRange(blockModel.GetFaceWithOffsetAO(world.WorldChunks, ChunkUtils.PositionToChunk(globalBlockPosition), BlockModelCullDirection.None, ChunkUtils.PositionToBlockLocal(globalBlockPosition)));
            
            if (!world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (0, 1, 0))].SolidMask[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition + (0, 1, 0)))] && blockModel.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Up))
            {

                world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].OpaqueMeshList.AddRange(blockModel.GetFaceWithOffsetAO(world.WorldChunks, ChunkUtils.PositionToChunk(globalBlockPosition), BlockModelCullDirection.Up, ChunkUtils.PositionToBlockLocal(globalBlockPosition)));
                
            }
            if (!world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (0, -1, 0))].SolidMask[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition + (0, -1, 0)))] && blockModel.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Down))
            {

                world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].OpaqueMeshList.AddRange(blockModel.GetFaceWithOffsetAO(world.WorldChunks, ChunkUtils.PositionToChunk(globalBlockPosition), BlockModelCullDirection.Down, ChunkUtils.PositionToBlockLocal(globalBlockPosition)));

            }
            if (!world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (1, 0, 0))].SolidMask[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition + (1, 0, 0)))] && blockModel.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Left))
            {

                world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].OpaqueMeshList.AddRange(blockModel.GetFaceWithOffsetAO(world.WorldChunks, ChunkUtils.PositionToChunk(globalBlockPosition), BlockModelCullDirection.Left, ChunkUtils.PositionToBlockLocal(globalBlockPosition)));

            }
            if (!world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (-1, 0, 0))].SolidMask[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition + (-1, 0, 0)))] && blockModel.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Right))
            {

                world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].OpaqueMeshList.AddRange(blockModel.GetFaceWithOffsetAO(world.WorldChunks, ChunkUtils.PositionToChunk(globalBlockPosition), BlockModelCullDirection.Right, ChunkUtils.PositionToBlockLocal(globalBlockPosition)));

            }
            if (!world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (0, 0, 1))].SolidMask[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition + (0, 0, 1)))] && blockModel.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Back))
            {

                world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].OpaqueMeshList.AddRange(blockModel.GetFaceWithOffsetAO(world.WorldChunks, ChunkUtils.PositionToChunk(globalBlockPosition), BlockModelCullDirection.Back, ChunkUtils.PositionToBlockLocal(globalBlockPosition)));

            }
            if (!world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition + (0, 0, -1))].SolidMask[ChunkUtils.VecToIndex(ChunkUtils.PositionToBlockLocal(globalBlockPosition + (0, 0, -1)))] && blockModel.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Front))
            {

                world.WorldChunks[ChunkUtils.PositionToChunk(globalBlockPosition)].OpaqueMeshList.AddRange(blockModel.GetFaceWithOffsetAO(world.WorldChunks, ChunkUtils.PositionToChunk(globalBlockPosition), BlockModelCullDirection.Front, ChunkUtils.PositionToBlockLocal(globalBlockPosition)));

            }

        }

    }
}
