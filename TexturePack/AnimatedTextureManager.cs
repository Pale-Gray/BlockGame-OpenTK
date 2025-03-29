using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Tomlet.Attributes;

namespace Game.Core.TexturePack;

public enum AnimatedTextureMode
{

    Loop,
    PingPong

}
public class AnimatedTextureProperties
{ 

    public Dictionary<int, byte[]> Frames;
    public Vector2i Size;
    public int TextureSlotIndex;
    public int CurrentFrameTick;
    [TomlProperty("duration")]
    public int DurationTicks { get; set; }
    [TomlProperty("is_forwards")]
    public bool IsMovingForwards { get; set; } = true;
    [TomlProperty("animation_mode")]
    public AnimatedTextureMode AnimationMode { get; set; } = AnimatedTextureMode.Loop;
    [TomlProperty("texture")]
    public string TextureHandle { get; set; }

    public AnimatedTextureProperties() {}

    public void Tick()
    {

        if (IsMovingForwards)
        {
            CurrentFrameTick++;
        } else 
        {
            CurrentFrameTick--;
        }

        if (CurrentFrameTick > DurationTicks)
        {
            if (AnimationMode == AnimatedTextureMode.Loop)
            {
                CurrentFrameTick = 0;
            }
            if (AnimationMode == AnimatedTextureMode.PingPong)
            {
                IsMovingForwards = false;
                CurrentFrameTick = DurationTicks - 1;
            }
        }

        if (CurrentFrameTick < 0)
        {
            if (AnimationMode == AnimatedTextureMode.Loop)
            {
                CurrentFrameTick = DurationTicks;
            }
            if (AnimationMode == AnimatedTextureMode.PingPong)
            {
                IsMovingForwards = true;
                CurrentFrameTick = 1;
            }
        }

        int currentFrameIndex = (int) Math.Floor(CurrentFrameTick / (float) DurationTicks * (Frames.Count - 1));
        GL.TextureSubImage3D(TexturePackManager.ArrayTextureHandle, 0, 0, 0, TextureSlotIndex, Size.X, Size.Y, 1, PixelFormat.Rgba, PixelType.UnsignedByte, Frames[currentFrameIndex]);

    }

}
public class AnimatedTextureManager
{

    public static Dictionary<string, int> AnimatedTextureIndices = new();
    public static List<AnimatedTextureProperties> AnimatexTextures = new();

    public static void Update()
    {

        for (int i = 0; i < AnimatexTextures.Count; i++)
        {
            AnimatexTextures[i].Tick();
        }

    }

}