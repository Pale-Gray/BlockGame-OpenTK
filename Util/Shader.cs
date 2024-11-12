using OpenTK.Graphics.OpenGL;
using System;
using System.IO;

using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK.Util
{
    internal class Shader
    {

        public int id;

        public Shader(string vertexFile, string fragmentFile)
        {

            int vertshader;
            int fragshader;

            string vert = File.ReadAllText(Path.Combine(GlobalValues.ShaderPath, vertexFile));
            string frag = File.ReadAllText(Path.Combine(GlobalValues.ShaderPath, fragmentFile));

            // creating shader and source
            vertshader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertshader, vert);
            fragshader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragshader, frag);

            // compiling
            GL.CompileShader(vertshader);

            // GL.GetShader(vertshader, , out int Cv);
            int Cv = GL.GetShaderi(vertshader, ShaderParameterName.CompileStatus);
            if (Cv == 0)
            {
                // Console.WriteLine(GL.GetShaderInfoLog(vertshader));
                GL.GetShaderInfoLog(vertshader, out string info);
                Console.WriteLine(info);
            }
            GL.CompileShader(fragshader);

            // GL.GetShader(fragshader, ShaderParameter.CompileStatus, out int Cf);
            if (GL.GetShaderi(fragshader, ShaderParameterName.CompileStatus) == 0)
            {
                //Console.WriteLine(GL.GetShaderInfoLog(fragshader));
                GL.GetShaderInfoLog(fragshader, out string info);
                Console.WriteLine(info);
            }

            // attaching to program
            id = GL.CreateProgram();
            GL.AttachShader(id, vertshader);
            GL.AttachShader(id, fragshader);

            GL.LinkProgram(id);
            // GL.GetProgram(id, GetProgramParameterName.LinkStatus, out int Cs);
            int Cs = GL.GetProgrami(id, ProgramProperty.LinkStatus);
            if (Cs == 0)
            {

                //Console.WriteLine(GL.GetProgramInfoLog(id));
                GL.GetProgramInfoLog(id, out string info);
                Console.WriteLine(info);
            }

            // cleanup
            GL.DetachShader(id, vertshader);
            GL.DetachShader(id, fragshader);
            GL.DeleteShader(vertshader);
            GL.DeleteShader(fragshader);

        }

        public void Use()
        {

            GL.UseProgram(id);

        }
        public void UnUse()
        {

            // GL.UseProgram(0);

        }

        public int getID()
        {

            return id;

        }


        // fancy stuff for disposing shader
        private bool disposev = false;
        protected virtual void Dispose(bool disposing)
        {

            if (!disposev)
            {

                GL.DeleteProgram(id);
                disposev = true;

            }

        }
        ~Shader()
        {

            if (disposev == false)
            {

                Console.WriteLine("GPU Resource Leak.. forget to call Dispose()?");

            }

        }
        public void Dispose()
        {

            Dispose(true);
            GC.SuppressFinalize(this);

        }

    }
}
