using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using OpenTK.Mathematics;
using Blockgame_OpenTK.Registry;
using System.IO;

namespace Blockgame_OpenTK.Util
{
    internal class GlobalValues
    {

        public static string TexturePath = "Resources/Textures/";
        public static string ShaderPath = "Resources/Shaders/";
        public static string MissingTexture = "Resources/Textures/missing.png";
        public static string LocalPath = Path.Combine(TexturePath, "TextureArray");
        public static string BlockDataPath = "Resources/Blocks/";
        public static string BlockModelPath = "Resources/Blocks/Models/";

        public const string PATH = "hello";

        public static float WIDTH = 640f;
        public static float HEIGHT = 480f;
        public static Vector2 Center = (WIDTH / 2f, HEIGHT / 2f);
        public const int ChunkSize = 32;
        public static double DeltaTime = 0;
        public static double Time = 0;
        public static MouseState Mouse = null;
        public static KeyboardState Keyboard = null;
        public static CursorState CursorState = CursorState.Normal;
        public static bool RenderAmbientOcclusion = true;
        public static bool ShouldRenderWireframe = false;

        public static readonly int PlayerRange = 10;

        public const float AtlasResolution = 8;
        public const float Ratio = 1 / AtlasResolution;

        public static int seed = new Random().Next();

        public static bool ShouldRenderFog = true;
        public static float FogOffset = 0.0f;
        public static bool ShouldRenderBounds = false;

        // public static long seed = 1245919872491;
        public static TextureAtlas AtlasTexture;
        public static ArrayTexture ArrayTexture = new ArrayTexture();
        public static Shader ChunkShader;
        public static Shader SkyboxShader;
        public static Shader DefaultShader;
        public static Shader GuiShader;
        public static Shader GuiBlockShader;

        public static ushort BlockSelectorID = 1;

        public static Camera GuiCamera = new Camera((0, 0, 1), (0, 0, -1), (0, 1, 0), CameraType.Orthographic, 90f);

        public static string FrameInformation = "";

        public static Register Register = new Register();

    }
}
