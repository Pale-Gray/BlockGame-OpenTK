using OpenTK.Graphics.OpenGL4;

namespace Blockgame_OpenTK.Util
{
    internal class Vbo
    {

        static int vbo;// = GL.GenBuffer();
        public static int Generate(float[] meshData, BufferUsageHint hint)
        {
            if (meshData == null)
            {

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, 1 * sizeof(float), new float[0], hint);

            } else
            {

                vbo = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, meshData.Length * sizeof(float), meshData, hint);

            }

            // GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            return vbo;

        }

        public static void Unbind()
        {

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        }

    }
}
