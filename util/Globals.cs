using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace Blockgame_OpenTK.Util
{
    internal class Globals
    {

        public static float WIDTH = 640f;
        public static float HEIGHT = 480f;
        public const int ChunkSize = 32;
        public static double DeltaTime = 0;
        public static double Time = 0;
        public static MouseState Mouse = null;
        public static KeyboardState Keyboard = null;

        public const float AtlasResolution = 8;
        public const float Ratio = 1 / AtlasResolution;

        public static int seed = new Random().Next();
        // public static long seed = 1245919872491;
        public static TextureAtlas AtlasTexture;
        public static Shader ChunkShader;
        public static Shader SkyboxShader;
        public static Shader DefaultShader;

        public static String FrameInformation = "";

        public static String TexturePath = "../../../Resources/Textures/";
        public static String ShaderPath = "../../../Resources/Shaders/";
        public static String MissingTexture = "../../../Resources/Textures/missing.png";

        public static FastNoiseLite noise = new FastNoiseLite();

    }
}
