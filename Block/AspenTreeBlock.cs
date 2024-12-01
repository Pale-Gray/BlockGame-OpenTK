using Blockgame_OpenTK.BlockProperty;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Gui;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
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

        private BlockModel _centerPiece12 = BlockModel.LoadFromJson(Path.Combine("AspenTree", "AspenTree12DiameterCenter.json"));

        public AspenTreeBlock()
        {

            SetProperties(_blockProperties);
            BlockProperties = new AspenTreeBlockProperties();
            
        }

        public override void OnBlockPlace(World world, Vector3i globalBlockPosition)
        {

            base.OnBlockPlace(world, globalBlockPosition);

        }

        public override void OnBlockMesh(World world, Dictionary<Vector3i, bool[]> mask, BlockProperty.BlockProperties properties, Vector3i globalBlockPosition)
        {

            AspenTreeBlockProperties blockProperties = (AspenTreeBlockProperties) properties;
            
            if (blockProperties.Thickness == 16)
            {

                world.AppendModel(_centerPiece16, globalBlockPosition, mask);

            }

            if (blockProperties.Thickness == 12)
            {

                world.AppendModel(_centerPiece12, globalBlockPosition, mask);

            }

        }

        public override void OnRandomTickUpdate(World world, Vector3i globalBlockPosition, BlockProperty.BlockProperties blockProperties)
        {

            AspenTreeBlockProperties properties = (AspenTreeBlockProperties) blockProperties;

            if (properties.MaxGrowthHeight > 0)
            {
                
                if (world.GetBlock(globalBlockPosition + Vector3i.UnitY) == Blocks.AirBlock)
                {

                    if (properties.MaxGrowthHeight < 10)
                    {

                        properties.Thickness = 12;

                    }

                    properties.MaxGrowthHeight--;
                    world.SetBlock(globalBlockPosition + Vector3i.UnitY, properties, GlobalValues.Register.GetIDFromBlock(this));

                }

            }

            /*
            if (blockProperties.TryGetProperty("maxGrowthHeight", out int maxGrowthHeight) && maxGrowthHeight > 0)
            {

                blockProperties.TryGetProperty("thickness", out double thickness);

                if (world.GetBlock(globalBlockPosition + Vector3i.UnitY) == Blocks.AirBlock)
                {

                    blockProperties.SetProperty("maxGrowthHeight", maxGrowthHeight - 1);
                    if (maxGrowthHeight == 5) blockProperties.SetProperty("thickness", 12.0);

                    world.SetBlock(globalBlockPosition + Vector3i.UnitY, blockProperties, GlobalValues.Register.GetIDFromBlock(this));

                }

            }
            */

        }

    }
}
