using System;
using Game.Util;

namespace Game.Core.Chrono;

public class Time
{

    public static float TicksToSeconds(int ticks) => ticks / 30.0f;
    public static float TicksToMinutes(int ticks) => TicksToSeconds(ticks) / 60.0f;
    public static float TicksToHours(int ticks) => TicksToMinutes(ticks) / 60.0f;
    public static int SecondsToTicks(float seconds) => (int) Math.Floor(seconds * 30.0);
    public static string TicksToDateTime(int ticks)
    {

        return $"{Math.Floor(TicksToHours(ticks) % 24):00}:{Math.Floor(TicksToMinutes(ticks) % 60):00}:{Math.Floor(TicksToSeconds(ticks) % 60):00}";

    }

}