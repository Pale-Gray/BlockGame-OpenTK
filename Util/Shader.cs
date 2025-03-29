using OpenTK.Graphics.OpenGL;
using System;
using System.IO;

using Game.Util;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Game.Util
{
    public class Shader
    {

        public int Handle { get; private set; }
        private Dictionary<string, int> _uniforms = new();

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
            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertshader);
            GL.AttachShader(Handle, fragshader);

            GL.LinkProgram(Handle);
            // GL.GetProgram(id, GetProgramParameterName.LinkStatus, out int Cs);
            int Cs = GL.GetProgrami(Handle, ProgramProperty.LinkStatus);
            if (Cs == 0)
            {

                //Console.WriteLine(GL.GetProgramInfoLog(id));
                GL.GetProgramInfoLog(Handle, out string info);
                Console.WriteLine(info);
            }

            // cleanup
            GL.DetachShader(Handle, vertshader);
            GL.DetachShader(Handle, fragshader);
            GL.DeleteShader(vertshader);
            GL.DeleteShader(fragshader);

        }

        public void Compile()
        {



        }

        public void Use()
        {

            GL.UseProgram(Handle);

        }

        public int GetUniformLocation(string uniformName)
        {

            if (_uniforms.TryGetValue(uniformName, out int value)) return value;

            int location = GL.GetUniformLocation(Handle, uniformName);
            _uniforms.Add(uniformName, location);
            return location;

        }

        // fancy stuff for disposing shader
        private bool disposev = false;
        protected virtual void Dispose(bool disposing)
        {

            if (!disposev)
            {

                GL.DeleteProgram(Handle);
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
