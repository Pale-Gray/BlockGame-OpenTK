using System;
using System.Runtime.InteropServices;
using Game.Audio;
using Game.Core.Chunks;
using Game.Core.Worlds;
using Game.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Game.BlockUtil 
{

    public class PlimboBlock : Block 
    {

        public override void OnBlockPlace(World world, Vector3i globalBlockPosition, bool e = true)
        {
            base.OnBlockPlace(world, globalBlockPosition);
            // world.AddLight(globalBlockPosition, new BlockLight(new LightColor(8, 0, 15)));
            AudioPlayer.PlaySoundGlobal("bird_tweet.ogg", globalBlockPosition, pitch: Maths.Lerp(0.75f, 1.5f, (float)new Random().NextDouble()));
        }

        public override void OnBlockDestroy(World world, Vector3i globalBlockPosition, bool e = true)
        {
            base.OnBlockDestroy(world, globalBlockPosition);
            AudioPlayer.PlaySoundGlobal("bird_tweet2.ogg", globalBlockPosition, pitch: Maths.Lerp(0.8f, 1.25f, (float)new Random().NextDouble()));
        }

    }

}