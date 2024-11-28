using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blockgame_OpenTK.Util
{
    internal class Rle
    {

        struct Pair
        {

            public uint Count;
            public uint Value;

            public Pair(uint count, uint value)
            {

                Count = count;
                Value = value;

            }

        }
        
        public static byte[] Compress<TSize, TData>(TData[] blockData)
        {

            return Array.Empty<byte>();

        }

        public static byte[] Compress(ushort[] blockData)
        {

            List<ushort> compressed = new List<ushort>();

            ushort count = 1;
            ushort value = blockData[0];

            for (int i = 1; i < blockData.Length; i++)
            {

                if (blockData[i] != value)
                {

                    compressed.Add(value);
                    compressed.Add(count);
                    count = 0;
                    value = blockData[i];

                }
                count++;

            }

            compressed.Add(value);
            compressed.Add(count);

            byte[] compressedBytes = new byte[compressed.Count*2];
            Buffer.BlockCopy(compressed.ToArray(), 0, compressedBytes, 0, compressed.Count*2);

            return compressedBytes;

        }

        public static ushort[] Decompress(byte[] rleData)
        {

            ushort[] encoded = new ushort[rleData.Length/2];
            Buffer.BlockCopy(rleData, 0, encoded, 0, rleData.Length);
            List<ushort> decompressed = new List<ushort>();
            
            for(int i = 0; i < encoded.Length; i+=2)
            {

                ushort value = encoded[i];
                ushort count = encoded[i+1];

                for (int c = 0; c < count; c++)
                {

                    decompressed.Add(value);

                }

            }

            return decompressed.ToArray();

        }

    }
}
