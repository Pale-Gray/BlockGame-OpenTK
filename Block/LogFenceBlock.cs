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
    internal class LogFenceBlock : Block
    {

        private static Block _properties = LoadFromJson("LogFenceBlock.json");
        private static BlockModel _post = BlockModel.LoadFromJson("LogFencePost.json");
        private static BlockModel _rightPortion = BlockModel.LoadFromJson("LogFencePostRight.json");
        private static BlockModel _frontPortion = BlockModel.LoadFromJson("LogFencePostFront.json");
        private static BlockModel _leftPortion = BlockModel.LoadFromJson("LogFencePostLeft.json");
        private static BlockModel _backPortion = BlockModel.LoadFromJson("LogFencePostBack.json");
        public LogFenceBlock()
        {

            SetProperties(_properties);
            GuiRenderableBlockModel = new Gui.GuiBlockModel(_post);

        }

        public override void OnBlockMesh(World world, Dictionary<Vector3i, bool[]> mask, BlockProperty.BlockProperties properties, Vector3i globalBlockPosition)
        {

            ChunkMeshHandler.AddModel(world, globalBlockPosition, _post);
            if (world.GetBlock(globalBlockPosition + Vector3i.UnitX) == GlobalValues.Register.GetBlockFromName("LogFenceBlock")) ChunkMeshHandler.AddModel(world, globalBlockPosition, _leftPortion);
            if (world.GetBlock(globalBlockPosition - Vector3i.UnitX) == GlobalValues.Register.GetBlockFromName("LogFenceBlock")) ChunkMeshHandler.AddModel(world, globalBlockPosition, _rightPortion);
            if (world.GetBlock(globalBlockPosition + Vector3i.UnitZ) == GlobalValues.Register.GetBlockFromName("LogFenceBlock")) ChunkMeshHandler.AddModel(world, globalBlockPosition, _backPortion);
            if (world.GetBlock(globalBlockPosition - Vector3i.UnitZ) == GlobalValues.Register.GetBlockFromName("LogFenceBlock")) ChunkMeshHandler.AddModel(world, globalBlockPosition, _frontPortion);

        }

    }
}
