

using OpenTK.Graphics.OpenGL;
using System;
using System.IO;

namespace Game.Util
{
    public class ComputeShader
    {

        public int Id;
        public ComputeShader(string filename)
        {

            string computerShader = File.ReadAllText(Path.Combine(GlobalValues.ShaderPath, filename));

            int computeShader = GL.CreateShader(ShaderType.ComputeShader);
            GL.ShaderSource(computeShader, computerShader);
            GL.CompileShader(computeShader);

            int compileStatus = GL.GetShaderi(computeShader, ShaderParameterName.CompileStatus);
            if (compileStatus == 0)
            {

                GL.GetShaderInfoLog(computeShader, out string info);
                GameLogger.Log(info, Severity.Error);

            }

            Id = GL.CreateProgram();
            GL.AttachShader(Id, computeShader);
            GL.LinkProgram(Id);

            int linkStatus = GL.GetProgrami(Id, ProgramProperty.LinkStatus);
            if (linkStatus == 0)
            {

                GL.GetProgramInfoLog(Id, out string info);
                GameLogger.Log(info, Severity.Error);

            }

        }

        public void Bind()
        {

            GL.UseProgram(Id);

        }

        public void Dispatch(uint groupsX, uint groupsY, uint groupsZ)
        {

            GL.DispatchCompute(groupsX, groupsY, groupsZ);

        }

    }
}
