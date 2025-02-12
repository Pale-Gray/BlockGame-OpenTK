using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Util
{
    internal class BlockLightColorConverter
    {

        public static Vector3i FromHex(string hex)
        {

            if (!hex.StartsWith("0x") || hex.Length != 8) return Vector3i.Zero;
            string hexR = hex.Substring(2, 2);
            string hexG = hex.Substring(4, 2);
            string hexB = hex.Substring(6, 2);

            uint uintR = (uint)(Math.Floor(15.0f * (float) ((Convert.ToUInt32(hexR, 16) / 15.0f))));
            uint uintG = (uint)(Math.Floor(15.0f * (float)((Convert.ToUInt32(hexG, 16) / 15.0f))));
            uint uintB = (uint)(Math.Floor(15.0f * (float)((Convert.ToUInt32(hexB, 16) / 15.0f))));

            GameLogger.Log($"{uintR}, {uintG}, {uintB}", Severity.Info);
            
            return ((int)uintR, (int)uintG, (int)uintB);
            
        }

        public static Vector3i FromRgb(byte r, byte b, byte g)
        {

            uint uintR = (uint)Math.Floor(15.0f * (r / 255.0f));
            uint uintG = (uint)Math.Floor(15.0f * (g / 255.0f));
            uint uintB = (uint)Math.Floor(15.0f * (b / 255.0f));

            GameLogger.Log($"{uintR}, {uintG}, {uintB}", Severity.Info);

            return ((int)uintR, (int)uintG, (int)uintB);

        }

        public static Vector3i FromNormalizedRgb(Vector3 normalizedRgb)
        {

            uint uintR = (uint)(15.0f * Math.Floor(normalizedRgb.X));
            uint uintG = (uint)(15.0f * Math.Floor(normalizedRgb.Y));
            uint uintB = (uint)(15.0f * Math.Floor(normalizedRgb.Z));

            GameLogger.Log($"{uintR}, {uintG}, {uintB}", Severity.Info);

            return ((int)uintR, (int)uintG, (int)uintB);

        }

        public static Vector3i Unpack(uint lightData)
        {

            uint r = (lightData >> 12) & 0xF;
            uint g = (lightData >> 8) & 0xF;
            uint b = (lightData >> 4) & 0xF;

            return ((int)r, (int)g, (int)b);

        }

        public static uint Pack(Vector3i lightData)
        {

            uint r = (ushort) (lightData.X & 0xF);
            uint g = (ushort) (lightData.Y & 0xF);
            uint b = (ushort) (lightData.Z & 0xF);

            uint data = 0;
            data = (ushort) (r << 12 | data);
            data = (ushort) (g << 8 | data);
            data = (ushort) (b << 4 | data);

            return data;

        }

    }
}
