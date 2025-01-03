using Blockgame_OpenTK.BlockProperty;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Gui;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Dynamic;
using System.IO;

namespace Blockgame_OpenTK.BlockUtil
{
    internal class AspenTreeBlock : Block
    {

        private Block _blockProperties = LoadFromJson("AspenTreeBlock.json");
        private BlockModel _centerPiece28 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree28DiameterCenter.json"));
        private BlockModel _topPiece28 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree28DiameterTop.json"));
        private BlockModel _bottomPiece28 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree28DiameterBottom.json"));
        private BlockModel _frontPiece28 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree28DiameterFront.json"));
        private BlockModel _leftPiece28 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree28DiameterLeft.json"));
        private BlockModel _backPiece28 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree28DiameterBack.json"));
        private BlockModel _rightPiece28 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree28DiameterRight.json"));

        private BlockModel _centerPiece16 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree16DiameterCenter.json"));
        private BlockModel _topPiece16 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree16DiameterTop.json"));
        private BlockModel _bottomPiece16 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree16DiameterBottom.json"));
        private BlockModel _leftPiece16 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree16DiameterLeft.json"));
        private BlockModel _rightPiece16 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree16DiameterRight.json"));
        private BlockModel _frontPiece16 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree16DiameterFront.json"));
        private BlockModel _backPiece16 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree16DiameterBack.json"));

        private BlockModel _centerPiece12 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree12DiameterCenter.json"));
        private BlockModel _topPiece12 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree12DiameterTop.json"));
        private BlockModel _bottomPiece12 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree12DiameterBottom.json"));
        private BlockModel _leftPiece12 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree12DiameterLeft.json"));
        private BlockModel _rightPiece12 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree12DiameterRight.json"));
        private BlockModel _frontPiece12 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree12DiameterFront.json"));
        private BlockModel _backPiece12 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree12DiameterBack.json"));

        public AspenTreeBlock()
        {

            BlockPropertiesType = typeof(AspenTreeBlockProperties);
            SetProperties(_blockProperties);
            BlockProperties = new AspenTreeBlockProperties();
            
        }

        public override void OnBlockPlace(World world, Vector3i globalBlockPosition)
        {

            world.SetBlock(globalBlockPosition, new AspenTreeBlockProperties(), this);

        }

        public override void OnBlockMesh(World world, Dictionary<Vector3i, uint[]> mask, IBlockProperties properties, Vector3i globalBlockPosition)
        {

            AspenTreeBlockProperties blockProperties = (AspenTreeBlockProperties) properties;

            // world.AppendModel(_topPiece12, globalBlockPosition, mask);
            // world.AppendModel(_bottomPiece12, globalBlockPosition, mask);
            world.AppendModel(_leftPiece16, globalBlockPosition, mask);
            // world.AppendModel(_rightPiece12, globalBlockPosition, mask);
            // world.AppendModel(_backPiece12, globalBlockPosition, mask);
            // world.AppendModel(_frontPiece12, globalBlockPosition, mask);

            /*
            if (blockProperties.Thickness == 16)
            {

                world.AppendModel(_centerPiece16, globalBlockPosition, mask);
                if (world.GetBlock(globalBlockPosition + Vector3i.UnitY) == this && blockProperties.CanConnectUpwards) world.AppendModel(_topPiece16, globalBlockPosition, mask);
                if (world.GetBlock(globalBlockPosition - Vector3i.UnitY) == this && blockProperties.CanConnectDownwards) world.AppendModel(_bottomPiece16, globalBlockPosition, mask);
                if (world.GetBlock(globalBlockPosition + Vector3i.UnitX) == this && blockProperties.CanConnectLeft) world.AppendModel(_leftPiece16, globalBlockPosition, mask);
                if (world.GetBlock(globalBlockPosition - Vector3i.UnitX) == this && blockProperties.CanConnectRight) world.AppendModel(_rightPiece16, globalBlockPosition, mask);
                if (world.GetBlock(globalBlockPosition + Vector3i.UnitZ) == this && blockProperties.CanConnectForwards) world.AppendModel(_backPiece16, globalBlockPosition, mask);
                if (world.GetBlock(globalBlockPosition - Vector3i.UnitZ) == this && blockProperties.CanConnectBackwards) world.AppendModel(_frontPiece16, globalBlockPosition, mask);

            }

            if (blockProperties.Thickness == 12)
            {

                world.AppendModel(_centerPiece12, globalBlockPosition, mask);
                if (world.GetBlock(globalBlockPosition + Vector3i.UnitY) == this && blockProperties.CanConnectUpwards) world.AppendModel(_topPiece12, globalBlockPosition, mask);
                if (world.GetBlock(globalBlockPosition - Vector3i.UnitY) == this && blockProperties.CanConnectDownwards) world.AppendModel(_bottomPiece12, globalBlockPosition, mask);
                if (world.GetBlock(globalBlockPosition + Vector3i.UnitX) == this && blockProperties.CanConnectLeft) world.AppendModel(_leftPiece12, globalBlockPosition, mask);
                if (world.GetBlock(globalBlockPosition - Vector3i.UnitX) == this && blockProperties.CanConnectRight) world.AppendModel(_rightPiece12, globalBlockPosition, mask);
                if (world.GetBlock(globalBlockPosition + Vector3i.UnitZ) == this && blockProperties.CanConnectForwards) world.AppendModel(_backPiece12, globalBlockPosition, mask);
                if (world.GetBlock(globalBlockPosition - Vector3i.UnitZ) == this && blockProperties.CanConnectBackwards) world.AppendModel(_frontPiece12, globalBlockPosition, mask);

            }
            */

        }

        public override void OnRandomTickUpdate(World world, Vector3i globalBlockPosition, IBlockProperties blockProperties)
        {

            AspenTreeBlockProperties properties = (AspenTreeBlockProperties)blockProperties;
            AspenTreeBlockProperties sendBlockProperties = new AspenTreeBlockProperties();
            sendBlockProperties.Thickness = properties.Thickness;
            sendBlockProperties.MaxHeight = properties.MaxHeight;
            sendBlockProperties.StumpHeight = properties.StumpHeight;

            switch (properties.GrowingDirection)
            {

                case BranchDirection.Up:

                    break;

            }
            
        }

    }
}
