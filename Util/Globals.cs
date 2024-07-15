using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using OpenTK.Mathematics;
using Blockgame_OpenTK.Registry;

namespace Blockgame_OpenTK.Util
{
    internal class Globals
    {

        public static string TexturePath = "../../../Resources/Textures/";
        public static string ShaderPath = "../../../Resources/Shaders/";
        public static string MissingTexture = "../../../Resources/Textures/missing.png";
        public static string LocalPath = TexturePath + "TextureArray";
        public static string BlockDataPath = "../../../Resources/Blocks/";
        public static string BlockModelPath = "../../../Resources/Blocks/Models/";

        public static float WIDTH = 640f;
        public static float HEIGHT = 480f;
        public static Vector2 Center = (WIDTH / 2f, HEIGHT / 2f);
        public const int ChunkSize = 32;
        public static double DeltaTime = 0;
        public static double Time = 0;
        public static MouseState Mouse = null;
        public static KeyboardState Keyboard = null;
        public static CursorState CursorState = CursorState.Normal;

        public static readonly int PlayerRange = 10;

        public const float AtlasResolution = 8;
        public const float Ratio = 1 / AtlasResolution;

        public static int seed = new Random().Next();
        // public static long seed = 1245919872491;
        public static TextureAtlas AtlasTexture;
        public static ArrayTexture ArrayTexture = new ArrayTexture();
        public static Shader ChunkShader;
        public static Shader SkyboxShader;
        public static Shader DefaultShader;

        public static string FrameInformation = "";

        public static FastNoiseLite noise = new FastNoiseLite();

        public static Register Register = new Register();

    }
}
