using System;
using Blockgame_OpenTK.Audio;
using Blockgame_OpenTK.Core.Worlds;
using Blockgame_OpenTK.Util;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.BlockUtil 
{

    public class MooingBlock : NewBlock 
    {

        public override void OnBlockPlace(PackedChunkWorld world, Vector3i globalBlockPosition)
        {
            base.OnBlockPlace(world, globalBlockPosition);
            // AudioPlayer.PlaySoundLocal("moo.wav", Maths.Lerp(0.5f, 2.0f, (float)new Random().NextDouble()), 1.0f);
            Vector3 position = globalBlockPosition;
            // AudioPlayer.PlaySoundGlobal("moo.wav", in position, Vector3.Zero, pitch: Maths.Lerp(0.5f, 2.0f, (float)new Random().NextDouble()));
            // AudioPlayer.PlaySoundGlobal("moomono.wav", globalBlockPosition);
            AudioPlayer.PlayMusicLocal("countdownogg.ogg");
        }

        public override void OnBlockDestroy(PackedChunkWorld world, Vector3i globalBlockPosition)
        {
            base.OnBlockDestroy(world, globalBlockPosition);
            // AudioPlayer.PlaySoundLocal("squish.wav", 1.0f, 1.0f);
            Vector3 position = globalBlockPosition;
            // AudioPlayer.PlaySoundGlobal("squish.wav", globalBlockPosition, Vector3.Zero);
        }

    }

}