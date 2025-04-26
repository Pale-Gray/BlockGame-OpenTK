using System;
namespace Game.Core.Chrono;

public class Time
{

    public static float TicksToSeconds(long ticks) => ticks / 30.0f;
    public static float TicksToMinutes(long ticks) => TicksToSeconds(ticks) / 60.0f;
    public static float TicksToHours(long ticks) => TicksToMinutes(ticks) / 60.0f;
    public static int SecondsToTicks(float seconds) => (int) Math.Floor(seconds * 30.0);
    public static string TicksToDateTime(long ticks)
    {

        float hour = TicksToHours(ticks);
        float minutes = TicksToMinutes(ticks);
        float seconds = TicksToSeconds(ticks);

        int gameHour = (int) Math.Floor((hour * 24) + 6) % 24;
        int gameMinute = (int) Math.Floor((minutes * 24) % 60);
        int gameSecond = (int) Math.Floor((seconds * 24) % 60);

        return $"{gameHour:00}:{gameMinute:00}:{gameSecond:00}";

    }

}