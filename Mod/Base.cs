using System.IO;
using Game.BlockUtil;
using Game.Core.Generation;
using Game.Util;

namespace Game.Core.Modding;

public class Base : IModLoader
{
    public static void OnLoad(Register register)
    {
        
        register.RegisterBlock("Game.Air", new Block() { IsSolid = false });
        register.RegisterBlock("Game.GrassBlock", new Block() { DisplayName = "Game.Block.GrassBlock", BlockModel = new BlockModel().AddCube(new Cube()).SetAllTextures(0, "GrassBlockTop").Generate() });
        register.RegisterBlock("Game.DirtBlock", new Block() { DisplayName = "Game.Block.DirtBlock", BlockModel = new BlockModel().AddCube(new Cube()).SetAllTextures(0, "DirtBlock").Generate() });
        register.RegisterBlock("Game.StoneBlock", new Block() { DisplayName = "Game.Block.StoneBlock", BlockModel = new BlockModel().AddCube(new Cube()).SetAllTextures(0, "Stone").Generate()});
        register.RegisterBlock("Game.ShortGrass", new Block() { DisplayName = "Game.Block.ShortGrass", IsSolid = false, BlockModel = BlockModels.CrossModel.Copy().Generate() });
        register.RegisterBlock("Game.RedMushroomMycelium", new RedMushroomMyceliumBlock() { DisplayName = "Game.Block.RedMushroomMycelium", BlockModel = new BlockModel().AddCube(new Cube()).SetAllTextures(0, "MyceliumInfestedSoil").Generate() });
        register.RegisterBlock("Game.RedMushroomStem", new RedMushroomStemBlock() { DisplayName = "Game.Block.RedMushroomStem" });
        register.RegisterBlock("Game.RedMushroomCap", new RedMushroomCapBlock() { DisplayName = "Game.Block.RedMushroomCap" });
        register.RegisterBlock("Game.AirIgnoreBlock", new Block() { DisplayName = "Game.Block.AirIgnoreBlock", BlockModel = new BlockModel().AddCube(new Cube()).SetAllTextures(0, "Birb").Generate() });
        register.RegisterBlock("Game.Fixed", new FixedTickBlock() { DisplayName = "Game.Block.Fixed", BlockModel = new BlockModel().AddCube(new Cube()).SetAllTextures(0, "Birb").Generate() });
        register.RegisterBlock("Game.TinyRedMushroom", new TinyRedMushroomBlock() { DisplayName = "Game.Block.TinyRedMushroom", IsSolid = false });
        register.RegisterBlock("Game.LightBlock", new LightBlock() { DisplayName = "Game.Block.LightBlock" });

        register.RegisterBiome("Game.RedMushroomBiome", new RedMushroomBiome());

    }

}