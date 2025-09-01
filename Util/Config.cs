using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Graphics.Vulkan;
using OpenTK.Platform;
using VoxelGame.Networking;
using VoxelGame.Rendering;
using VoxelGame.Util;

namespace VoxelGame;

public class Config
{
    public const int ChunkSize = 32;
    public const int ChunkVolume = ChunkSize * ChunkSize * ChunkSize;
    public const int ColumnSize = 16;

    public static int Width = 900;
    public static int Height = 500;

    public static bool IsRunning = true;
    public static Register Register = new Register();
    
    public static DynamicAtlas Atlas;
    public static Shader ChunkShader;
    public static DeferredFramebuffer Framebuffer;
    public static Random Random = new Random();

    public static Server? Server;
    public static Client? Client;

    public static WindowHandle Window;
    public static OpenGLContextHandle OpenGLContext;

    public static int LastTicksPerSecond = 0;
    public static int TickSpeed = 30;
    public static float TickRate = 1.0f / TickSpeed;
    public static float DeltaTime;
    public static float ElapsedTime = 0;
    public static float TickInterpolant => (ElapsedTime % TickRate) / TickRate;
    public static Stopwatch Timer = new Stopwatch();
    public static float LastGenTime = 0.0f;
    public static List<float> GenTimes = [0];
    public static float LastMeshTime = 0.0f;
    public static List<float> MeshTimes = [0];

    public static List<float> FrameTimesOfLastSecond = new();
    public static float MinimumFps;
    public static float MaximumFps;
    public static float AverageFps;
}