using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.Vulkan;
using OpenTK.Mathematics;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.ChunkUtil
{
    internal unsafe class ChunkSerializer
    {

        public static void Serialize(Chunk chunk)
        {

            using (FileStream stream = File.Create(Path.Combine("Chunks", chunk.FileName)))
            {

                stream.Write(Encoding.UTF8.GetBytes("CK0")); // file validator
                stream.Write(Encoding.UTF8.GetBytes("LHT")); // indicates light data

                int length = chunk.GlobalBlockLightPositions.Count;
                ReadOnlySpan<byte> count = MemoryMarshal.AsBytes(new Span<int>(ref length));
                stream.Write(count);
                foreach (KeyValuePair<Vector3i, Vector3i> pair in chunk.GlobalBlockLightPositions)
                {

                    KeyValuePair<Vector3i, Vector3i> keyval = pair;
                    ReadOnlySpan<byte> keyValuePair = MemoryMarshal.AsBytes(new Span<KeyValuePair<Vector3i, Vector3i>>(ref keyval));

                }
                stream.Write(Encoding.UTF8.GetBytes("BLK"));
                byte[] rleData = Rle.Compress(chunk.BlockData);
                stream.Write(BitConverter.GetBytes((uint) (rleData.Length * sizeof(byte))));
                stream.Write(rleData);
                stream.Write(Encoding.UTF8.GetBytes("END"));

            }

        }

        public static void Deserialize(Chunk chunk)
        {

            if (File.Exists(Path.Combine("Chunks", chunk.FileName)))
            {

                using (FileStream stream = File.OpenRead(Path.Combine("Chunks", chunk.FileName)))
                {

                    byte[] header = new byte[3];
                    stream.Read(header, 0, 3);
                    byte[] block = new byte[3];
                    stream.Read(block, 0, 3);
                    byte[] length = new byte[sizeof(int)];
                    stream.Read(length, 0, sizeof(int));
                    int len = BitConverter.ToInt32(length);
                    byte[] lightPositionData = new byte[len];
                    stream.Read(lightPositionData, 0, len);
                    Dictionary<Vector3i, Vector3i> lightData = new Dictionary<Vector3i, Vector3i>();
                    // MemoryMarshal.Cast<byte[], Dictionary<Vector3i, Vector3i>>(new ReadOnlySpan<byte[]>(ref lightPositionData));

                }

            }

        }

        private static Dictionary<Vector3i, Vector3i> DeserializeLightPositions(byte[] bytePositions)
        {

            Dictionary<Vector3i, Vector3i> positions = new Dictionary<Vector3i, Vector3i>();

            for (int i = 0; i < bytePositions.Length; i += 2*sizeof(Vector3i))
            {

                int keyX = BitConverter.ToInt32(bytePositions, i);
                int keyY = BitConverter.ToInt32(bytePositions, i + sizeof(int));
                int keyZ = BitConverter.ToInt32(bytePositions, i + (2 * sizeof(int)));

                int valyeX = BitConverter.ToInt32(bytePositions, i + (3 * sizeof(int)));
                int valueY = BitConverter.ToInt32(bytePositions, i + (4 * sizeof(int)));
                int valueZ = BitConverter.ToInt32(bytePositions, i + (5 * sizeof(int)));

                positions.Add((keyX,keyY,keyZ),(valyeX,valueY,valueZ));

            }

            return positions;

        }

    }
}
