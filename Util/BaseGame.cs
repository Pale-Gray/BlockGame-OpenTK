namespace VoxelGame.Util;

public class BaseGame : IMod
{
    public static void OnLoad()
    {
        Config.Register.RegisterBlock("air", new Block() { IsSolid = false });
        Config.Register.RegisterBlock("grass", 
            new Block()
                .SetBlockModel(new BlockModel()
                    .AddCube(new Cube())
                    .SetTextureFace(0, Direction.Top, "chicken_systems")
                    .SetTextureSides(0, "chicken_systems")
                    .SetTextureFace(0, Direction.Bottom, "chicken_systems")));
        Config.Register.RegisterBlock("stone", new Block().SetBlockModel(new BlockModel().AddCube(new Cube()).SetAllTextureFaces(0, "stone")));
        Config.Register.RegisterBlock("sand",
            new Block() { IsSolid = true }
                .SetBlockModel(new BlockModel()
                    .AddCube(new Cube((0, 0, 0), (1, 1, 1)))
                    .SetAllTextureFaces(0, "sand")));
        Config.Register.RegisterBlock("pumpkin",
            new Block()
                .SetBlockModel(new BlockModel()
                    .AddCube(new Cube())
                    .SetTextureFace(0, Direction.Top, "pumpkin_top")
                    .SetTextureFace(0, Direction.Bottom, "pumpkin_bottom")
                    .SetTextureSides(0, "pumpkin_face")));
        Config.Register.RegisterBlock("water",
            new Block() { IsTransparent = true }.SetBlockModel(new BlockModel().AddCube(new Cube()).SetAllTextureFaces(0, "water")));
    }
}