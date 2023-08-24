using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj.util
{
    internal class Vbo
    {

        public static int Generate(float[] meshData, BufferUsageHint hint)
        {

            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, meshData.Length * sizeof(float), meshData, hint);
            // GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            return vbo;

        }

        public static void Unbind()
        {

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        }

    }
}
