using System;
using OpenTK.Platform;
using VoxelGame.Networking;
using VoxelGame.Util;

namespace VoxelGame;

public class Config
{
    public const int ChunkSize = 32;
    public const int ChunkVolume = ChunkSize * ChunkSize * ChunkSize;
    public const int ColumnSize = 8;

    public static int Width = 900;
    public static int Height = 500;
    public static float DeltaTime;

    public static bool IsRunning = true;
    public static Register Register = new Register();
    
    public static DynamicAtlas Atlas;
    public static Shader ChunkShader;
    public static Random Random = new Random();

    public static Server? Server;
    public static Client? Client;

    public static WindowHandle Window;
    public static OpenGLContextHandle OpenGLContext;
}