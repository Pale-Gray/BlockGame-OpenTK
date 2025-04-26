using OpenTK.Mathematics;
using System.IO;
using System.Collections.Generic;
using Game.Core.Networking;
using Game.Core.Modding;
using Game.Core.Worlds;
using System;
using Game.BlockUtil;
using OpenTK.Graphics.OpenGL;

namespace Game.Util
{

    public struct GameSettings
    {

        public Dictionary<string, string> Keymap { get; set; }
        public float MouseSensitivity { get; set; }

    }

    public class GameState
    {

        public static World World;

    }   

    public class NetworkingValues
    {

        public static Server Server;
        public static Client Client;

    }

    public class BlockModels
    {

        public static readonly BlockModel CrossModel = 
            new BlockModel()
            .AddCube(new Cube((0, 0, 8), (16, 16, 8)))
            .SetRotation(0, (0, 45, 0))
            .AddCube(new Cube((8, 0, 0), (8, 16, 16)))
            .SetRotation(1, (0, 45, 0));

    }
    public class GlobalValues
    {

        // public static IModLoader Base = new Base();
        public static string TexturePath => Path.Combine("Resources", "Textures") + Path.DirectorySeparatorChar; //"Resources/Textures/";
        public static string ShaderPath => Path.Combine("Resources", "Shaders") + Path.DirectorySeparatorChar;//"Resources/Shaders/";
        public static string MissingTexture => Path.Combine("Resources", "Textures", "missing.png");//"Resources/Textures/missing.png";
        public static string LocalPath => Path.Combine("Resources", "Textures", "TextureArray") + Path.DirectorySeparatorChar;
        public static string BlockDataPath => Path.Combine("Resources", "Data", "Blocks") + Path.DirectorySeparatorChar;//"Resources/Blocks/";
        public static string BlockModelPath => Path.Combine("Resources", "Data", "Blocks", "Models") + Path.DirectorySeparatorChar;//"Resources/Blocks/Models/";
        public static string TexturePackPath => Path.Combine("TexturePacks");

        public static List<string> AvailableExtensions = new();

        public static Texture MissingTexturePackIcon;

        public static List<string> LogMessages = new List<string>();

        public static GameSettings Settings;// JsonSerializer.Deserialize<GameSettings>(File.ReadAllText("settings.json"));

        public static float Width = 640f;
        public static float Height = 480f;
        public static Vector2 Center = (Width / 2f, Height / 2f);
        public const int ChunkSize = 32;
        public static double PreviousTime = 0;
        public static double CurrentTime = 0;
        public static double DeltaTime = 0;
        public static double Time = 0;

        public static bool IsRunning = true;

        public static bool ShouldHideHud = false;

        public static int AverageFps = 0;
        public static bool RenderAmbientOcclusion = true;
        public static bool ShouldRenderWireframe = false;
        public static bool IsCursorLocked = true;

        public static readonly int PlayerRange = 10;

        public static bool ShouldRenderFog = true;
        public static float FogOffset = 0.0f;
        public static bool ShouldRenderBounds = false;

        public static Register Register = new Register();
        public static Shader ChunkShader;
        public static Shader DefaultShader;
        public static Shader GuiShader;
        public static Shader GuiBlockShader;
        public static Shader CachedFontShader;

        public static Shader PackedChunkShader;
        public static Shader SkyboxShader;

        public static ushort BlockSelectorID = 1;

        public static Camera GuiCamera = new Camera((0, 0, 1), (0, 0, -1), (0, 1, 0), CameraType.Orthographic, 90f);

        public static readonly float TickRate = 30;
        public static readonly float GameDayTicks = 36000;
        public static Random RandomGenerator = new Random();

    }
}
