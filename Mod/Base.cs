using System.IO;
using Game.BlockUtil;
using Game.Registry;

namespace Game.Core.Modding;

public class Base : IModLoader
{
    public static void OnLoad(Register register)
    {
        
        register.RegisterBlock("Game.Air", new Block() { IsSolid = false });
        register.RegisterBlock("Game.GrassBlock", Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "grass_block.toml")));
        register.RegisterBlock("Game.DirtBlock", Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "dirt_block.toml")));
        register.RegisterBlock("Game.StoneBlock", Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "stone_block.toml")));
        register.RegisterBlock("Game.BrickBlock", Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "bricks.toml")));
        register.RegisterBlock("Game.PlimboBlock", Block.FromToml<PlimboBlock>(Path.Combine("Resources", "Data", "Blocks", "plimbo_block.toml")));
        register.RegisterBlock("Game.EmptyCrate", Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "empty_crate.toml")));
        register.RegisterBlock("Game.TomatoCrate", Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "tomato_crate.toml")));
        register.RegisterBlock("Game.RedLightBlock", Block.FromToml<RedLightBlock>(Path.Combine("Resources", "Data", "Blocks", "red_light_block.toml")));
        register.RegisterBlock("Game.GreenLightBlock", Block.FromToml<GreenLightBlock>(Path.Combine("Resources", "Data", "Blocks", "green_light_block.toml")));
        register.RegisterBlock("Game.BlueLightBlock", Block.FromToml<BlueLightBlock>(Path.Combine("Resources", "Data", "Blocks", "blue_light_block.toml")));
        register.RegisterBlock("Game.LightBlock", Block.FromToml<LightBlock>(Path.Combine("Resources", "Data", "Blocks", "light_block.toml")));
        register.RegisterBlock("Game.LeafBlock", Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "leaf_block.toml")));
        register.RegisterBlock("Game.GhastlingBlock", Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "ghastling_block.toml")));
        register.RegisterBlock("Game.AspenLog", Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "aspen_log.toml")));
        register.RegisterBlock("Game.ThinBlock", Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "thin_block.toml")));
        register.RegisterBlock("Game.RedMushroom", Block.FromToml<TinyMushroomBlock>(Path.Combine("Resources", "Data", "Blocks", "tiny_red_mushroom.toml")));
        register.RegisterBlock("Game.StairBlock", Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "stair_block.toml")));
        register.RegisterBlock("Game.ShortGrass", Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "short_grass.toml")));
    }

}