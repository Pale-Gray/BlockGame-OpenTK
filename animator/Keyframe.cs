using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using opentk_proj.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj.animator
{
    internal class Keyframe
    {
        bool IsRunning = true;
        float From, To;
        float DurationInSeconds;
        float Time = 0;
        float Result;
        public Keyframe(float from, float to, float durationInSeconds)
        {

            From = from;
            To = to;
            Result = From;
            DurationInSeconds = durationInSeconds;

        }

        public void Play()
        {

            if (Time > DurationInSeconds)
            {

                IsRunning = false;

            }
            if (IsRunning)
            {

                Result = Maths.Lerp(From, To, Time/DurationInSeconds);
                Time += (float) Constants.Time;

            }

        }

        public float GetResult()
        {

            return Result;

        }

    }
}
