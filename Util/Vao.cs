using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Util
{

    public enum AttribPointerMode
    {

        VertexNormalTexcoord,
        VertexNormal,
        VertexTexcoord,
        Vertex,
        Chunk

    };
    internal class Vao
    {

        public static int Generate(AttribPointerMode type)
        {

            int vao;

            switch (type)
            {

                case AttribPointerMode.Chunk:
                    vao = GL.GenVertexArray();
                    GL.BindVertexArray(vao);
                    GL.VertexAttribPointer(0, 1, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0 * sizeof(float)); // this is the blocktype data
                    GL.EnableVertexAttribArray(0);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 1 * sizeof(float)); // this is the vertices
                    GL.EnableVertexAttribArray(1);
                    GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 4 * sizeof(float)); // this is the normals
                    GL.EnableVertexAttribArray(2);
                    GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 7 * sizeof(float)); // UVs 
                    GL.EnableVertexAttribArray(3);
                    // GL.BindVertexArray(0);
                    return vao;
                case AttribPointerMode.VertexNormalTexcoord:
                    vao = GL.GenVertexArray();
                    GL.BindVertexArray(vao);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0 * sizeof(float));
                    GL.EnableVertexAttribArray(0);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
                    GL.EnableVertexAttribArray(1);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
                    GL.EnableVertexAttribArray(2);
                    // GL.BindVertexArray(0);
                    return vao;
                case AttribPointerMode.VertexNormal:
                    vao = GL.GenVertexArray();
                    GL.BindVertexArray(vao);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0 * sizeof(float));
                    GL.EnableVertexAttribArray(0);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
                    GL.EnableVertexAttribArray(1);
                    // GL.BindVertexArray(0);
                    return vao;
                case AttribPointerMode.VertexTexcoord:
                    vao = GL.GenVertexArray();
                    GL.BindVertexArray(vao);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0 * sizeof(float));
                    GL.EnableVertexAttribArray(0);
                    GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
                    GL.EnableVertexAttribArray(1);
                    // GL.BindVertexArray(0);
                    return vao;
                case AttribPointerMode.Vertex:
                    vao = GL.GenVertexArray();
                    GL.BindVertexArray(vao);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0 * sizeof(float));
                    GL.EnableVertexAttribArray(0);
                    return vao;
                default:
                    vao = GL.GenVertexArray();
                    GL.BindVertexArray(vao);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0 * sizeof(float));
                    GL.EnableVertexAttribArray(0);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
                    GL.EnableVertexAttribArray(1);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
                    GL.EnableVertexAttribArray(2);
                    // GL.BindVertexArray(0);
                    return vao;

            }

        }

        public static void Unbind()
        {

            GL.BindVertexArray(0);

        }

    }
}
