using System.IO;
using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Registry;

namespace Blockgame_OpenTK.Core.Modding;

public class Base : IModLoader
{
    public void OnLoad(NewRegister register)
    {
        
        register.RegisterBlock(new Namespace("Game", "Air"), new NewBlock());
        register.RegisterBlock(new Namespace("Game", "GrassBlock"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "grass_block.toml")));
        register.RegisterBlock(new Namespace("Game", "DirtBlock"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "dirt_block.toml")));
        register.RegisterBlock(new Namespace("Game", "StoneBlock"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "stone_block.toml")));
        register.RegisterBlock(new Namespace("Game", "BrickBlock"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "bricks.toml")));
        register.RegisterBlock(new Namespace("Game", "PlimboBlock"), NewBlock.FromToml<PlimboBlock>(Path.Combine("Resources", "Data", "Blocks", "plimbo_block.toml")));
        register.RegisterBlock(new Namespace("Game", "EmptyCrate"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "empty_crate.toml")));
        register.RegisterBlock(new Namespace("Game", "TomatoCrate"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "tomato_crate.toml")));
        register.RegisterBlock(new Namespace("Game", "RedLightBlock"), NewBlock.FromToml<RedLightBlock>(Path.Combine("Resources", "Data", "Blocks", "red_light_block.toml")));
        register.RegisterBlock(new Namespace("Game", "GreenLightBlock"), NewBlock.FromToml<GreenLightBlock>(Path.Combine("Resources", "Data", "Blocks", "green_light_block.toml")));
        register.RegisterBlock(new Namespace("Game", "BlueLightBlock"), NewBlock.FromToml<BlueLightBlock>(Path.Combine("Resources", "Data", "Blocks", "blue_light_block.toml")));
        register.RegisterBlock(new Namespace("Game", "LightBlock"), NewBlock.FromToml<LightBlock>(Path.Combine("Resources", "Data", "Blocks", "light_block.toml")));
        register.RegisterBlock(new Namespace("Game", "LeafBlock"), NewBlock.FromToml<NewBlock>(Path.Combine("Resources", "Data", "Blocks", "leaf_block.toml")));
        
    }

}