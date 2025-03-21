using System.IO;
using Game.BlockUtil;
using Game.Registry;

namespace Game.Core.Modding;

public class Base : IModLoader
{
    public void OnLoad(Register register)
    {
        
        register.RegisterBlock(new Namespace("Game", "Air"), new Block());
        register.RegisterBlock(new Namespace("Game", "GrassBlock"), Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "grass_block.toml")));
        register.RegisterBlock(new Namespace("Game", "DirtBlock"), Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "dirt_block.toml")));
        register.RegisterBlock(new Namespace("Game", "StoneBlock"), Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "stone_block.toml")));
        register.RegisterBlock(new Namespace("Game", "BrickBlock"), Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "bricks.toml")));
        register.RegisterBlock(new Namespace("Game", "PlimboBlock"), Block.FromToml<PlimboBlock>(Path.Combine("Resources", "Data", "Blocks", "plimbo_block.toml")));
        register.RegisterBlock(new Namespace("Game", "EmptyCrate"), Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "empty_crate.toml")));
        register.RegisterBlock(new Namespace("Game", "TomatoCrate"), Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "tomato_crate.toml")));
        register.RegisterBlock(new Namespace("Game", "RedLightBlock"), Block.FromToml<RedLightBlock>(Path.Combine("Resources", "Data", "Blocks", "red_light_block.toml")));
        register.RegisterBlock(new Namespace("Game", "GreenLightBlock"), Block.FromToml<GreenLightBlock>(Path.Combine("Resources", "Data", "Blocks", "green_light_block.toml")));
        register.RegisterBlock(new Namespace("Game", "BlueLightBlock"), Block.FromToml<BlueLightBlock>(Path.Combine("Resources", "Data", "Blocks", "blue_light_block.toml")));
        register.RegisterBlock(new Namespace("Game", "LightBlock"), Block.FromToml<LightBlock>(Path.Combine("Resources", "Data", "Blocks", "light_block.toml")));
        register.RegisterBlock(new Namespace("Game", "LeafBlock"), Block.FromToml<Block>(Path.Combine("Resources", "Data", "Blocks", "leaf_block.toml")));
        
    }

}