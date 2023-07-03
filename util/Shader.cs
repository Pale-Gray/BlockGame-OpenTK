using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opentk_proj
{
    internal class Shader
    {

        public int id;

        public Shader(string pathtovert, string pathtofrag)
        {

            int vertshader;
            int fragshader;

            string vert = File.ReadAllText(pathtovert);
            string frag = File.ReadAllText(pathtofrag);

            // creating shader and source
            vertshader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertshader, vert);
            fragshader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragshader, frag);

            // compiling
            GL.CompileShader(vertshader);

            GL.GetShader(vertshader, ShaderParameter.CompileStatus, out int Cv);
            if (Cv == 0)
            {
                Console.WriteLine(GL.GetShaderInfoLog(vertshader));
            }
            GL.CompileShader(fragshader);

            GL.GetShader(fragshader, ShaderParameter.CompileStatus, out int Cf);
            if (Cf == 0)
            {
                Console.WriteLine(GL.GetShaderInfoLog(fragshader));
            }

            // attaching to program
            id = GL.CreateProgram();
            GL.AttachShader(id, vertshader);
            GL.AttachShader(id, fragshader);

            GL.LinkProgram(id);
            GL.GetProgram(id, GetProgramParameterName.LinkStatus, out int Cs);
            if (Cs == 0)
            {

                Console.WriteLine(GL.GetProgramInfoLog(id));

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

            GL.UseProgram(0);

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
