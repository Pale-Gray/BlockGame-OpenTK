using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using OpenTK.Mathematics;
using Blockgame_OpenTK.Registry;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Collections;
using Blockgame_OpenTK.Gui;
using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.PlayerUtil;

namespace Blockgame_OpenTK.Util
{

    public struct GameSettings
    {

        public Dictionary<string, string> Keymap { get; set; }
        public float MouseSensitivity { get; set; }

    }
    internal class GlobalValues
    {

        public static string TexturePath = Path.Combine("Resources", "Textures") + Path.DirectorySeparatorChar; //"Resources/Textures/";
        public static string ShaderPath = Path.Combine("Resources", "Shaders") + Path.DirectorySeparatorChar;//"Resources/Shaders/";
        public static string MissingTexture = Path.Combine("Resources", "Textures", "missing.png") + Path.DirectorySeparatorChar;//"Resources/Textures/missing.png";
        public static string LocalPath = Path.Combine("Resources", "Textures", "TextureArray") + Path.DirectorySeparatorChar;
        public static string BlockDataPath = Path.Combine("Resources", "Data", "Blocks") + Path.DirectorySeparatorChar;//"Resources/Blocks/";
        public static string BlockModelPath = Path.Combine("Resources", "Data", "Blocks", "Models") + Path.DirectorySeparatorChar;//"Resources/Blocks/Models/";

        public static List<string> LogMessages = new List<string>();

        public static GameSettings Settings;// JsonSerializer.Deserialize<GameSettings>(File.ReadAllText("settings.json"));

        public static float WIDTH = 640f;
        public static float HEIGHT = 480f;
        public static Vector2 Center = (WIDTH / 2f, HEIGHT / 2f);
        public const int ChunkSize = 32;
        public static double PreviousTime = 0;
        public static double CurrentTime = 0;
        public static double DeltaTime = 0;
        public static double Time = 0;

        public static bool IsRunning = true;

        public static bool ShouldHideHud = false;

        public static int AverageFps = 0;

        public static List<GuiElement> GlobalGuiElements = new List<GuiElement>();
        public static bool RenderAmbientOcclusion = true;
        public static bool ShouldRenderWireframe = false;
        public static bool IsCursorLocked = true;

        public static readonly int PlayerRange = 10;

        public static bool ShouldRenderFog = true;
        public static float FogOffset = 0.0f;
        public static bool ShouldRenderBounds = false;

        public static NewRegister NewRegister = new NewRegister();
        public static Shader ChunkShader;
        public static Shader DefaultShader;
        public static Shader GuiShader;
        public static Shader GuiBlockShader;
        public static Shader CachedFontShader;
        public static Shader GuiLineShader;

        public static Shader PackedChunkShader;

        public static ushort BlockSelectorID = 1;

        public static Camera GuiCamera = new Camera((0, 0, 1), (0, 0, -1), (0, 1, 0), CameraType.Orthographic, 90f);

    }
}
