using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Gui;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.BlockUtil
{
    internal class LogBranchBlock : Block
    {

        private Block _blockProperties = LoadFromJson("LogBranchBlock.json");
        private BlockModel _centerPiece = BlockModel.LoadFromJson("LogBranchBlockCenter.json");
        private BlockModel _topPiece = BlockModel.LoadFromJson("LogBranchBlockUp.json");
        private BlockModel _bottomPiece = BlockModel.LoadFromJson("LogBranchBlockBottom.json");
        private BlockModel _leftPiece = BlockModel.LoadFromJson("LogBranchBlockLeft.json");
        private BlockModel _rightPiece = BlockModel.LoadFromJson("LogBranchBlockRight.json");
        private BlockModel _frontPiece = BlockModel.LoadFromJson("LogBranchBlockFront.json");
        private BlockModel _backPiece = BlockModel.LoadFromJson("LogBranchBlockBack.json");

        public LogBranchBlock()
        {

            SetProperties(_blockProperties);
            GuiRenderableBlockModel = new GuiBlockModel(_centerPiece);

        }

        public override void OnBlockMesh(World world, BlockProperties properties, Vector3i globalBlockPosition)
        {

            ChunkMeshHandler.AddModel(world, globalBlockPosition, _centerPiece);

            if (world.GetBlock(globalBlockPosition + Vector3i.UnitY) == Blocks.LogBranchBlock)
            {

                ChunkMeshHandler.AddModel(world, globalBlockPosition, _topPiece);

            }
            if (world.GetBlock(globalBlockPosition - Vector3i.UnitY) == Blocks.LogBranchBlock)
            {

                ChunkMeshHandler.AddModel(world, globalBlockPosition, _bottomPiece);

            }
            if (world.GetBlock(globalBlockPosition + Vector3i.UnitX) == Blocks.LogBranchBlock)
            {

                ChunkMeshHandler.AddModel(world, globalBlockPosition, _leftPiece);

            }
            if (world.GetBlock(globalBlockPosition - Vector3i.UnitX) == Blocks.LogBranchBlock)
            {

                ChunkMeshHandler.AddModel(world, globalBlockPosition, _rightPiece);

            }
            if (world.GetBlock(globalBlockPosition + Vector3i.UnitZ) == Blocks.LogBranchBlock)
            {

                ChunkMeshHandler.AddModel(world, globalBlockPosition, _backPiece);

            }
            if (world.GetBlock(globalBlockPosition - Vector3i.UnitZ) == Blocks.LogBranchBlock)
            {

                ChunkMeshHandler.AddModel(world, globalBlockPosition, _frontPiece);

            }

        }

    }
}
