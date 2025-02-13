using System;
using Blockgame_OpenTK.Audio;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.BlockUtil 
{

    public class PlimboBlock : NewBlock 
    {

        public override void OnBlockPlace(PackedChunkWorld world, Vector3i globalBlockPosition)
        {
            base.OnBlockPlace(world, globalBlockPosition);
            AudioPlayer.PlaySoundGlobal("bird_tweet.ogg", globalBlockPosition, pitch: Maths.Lerp(0.75f, 1.5f, (float)new Random().NextDouble()));
        }

        public override void OnBlockDestroy(PackedChunkWorld world, Vector3i globalBlockPosition)
        {
            base.OnBlockDestroy(world, globalBlockPosition);
            AudioPlayer.PlaySoundGlobal("bird_tweet2.ogg", globalBlockPosition, pitch: Maths.Lerp(0.8f, 1.25f, (float)new Random().NextDouble()));
        }

    }

}