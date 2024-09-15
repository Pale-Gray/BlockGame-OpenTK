using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Blockgame_OpenTK.Gui
{
    internal class GuiBlockModel
    {

        Matrix4 ModelMatrix = Matrix4.Identity;
        ChunkVertex[] Vertices;
        int Vao = 0, Vbo = 0;
        public GuiBlockModel(Block block)
        {

            List<ChunkVertex> vertices = new List<ChunkVertex>();

            // Console.WriteLine(block.BlockModel == null);

            if (block.BlockModel != null)
            {

                vertices.AddRange(block.BlockModel.ChunkReadableFaces[BlockModelCullDirection.Up]);
                vertices.AddRange(block.BlockModel.ChunkReadableFaces[BlockModelCullDirection.Down]);
                vertices.AddRange(block.BlockModel.ChunkReadableFaces[BlockModelCullDirection.Left]);
                vertices.AddRange(block.BlockModel.ChunkReadableFaces[BlockModelCullDirection.Right]);
                vertices.AddRange(block.BlockModel.ChunkReadableFaces[BlockModelCullDirection.Back]);
                vertices.AddRange(block.BlockModel.ChunkReadableFaces[BlockModelCullDirection.Front]);
                // vertices.AddRange(block.BlockModel.ChunkReadableFaces[BlockModelCullDirection.None]);

            }

            Vertices = vertices.ToArray();

            for (int i = 0; i < Vertices.Length; i++)
            {

                Vertices[i].Position -= (0.5f, 0.5f, 0.5f);

                Vertices[i].Position.X *= -1;
                Vertices[i].Position.Y *= -1;

                Vertices[i].Normal.Y *= -1;
                Vertices[i].Normal.X *= -1;

            }

            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);
            Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * Marshal.SizeOf<ChunkVertex>(), Vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 1, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.TextureIndex)));
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.Position)));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.TextureCoordinates)));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.Normal)));
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false, Marshal.SizeOf<ChunkVertex>(), Marshal.OffsetOf<ChunkVertex>(nameof(ChunkVertex.AmbientValue)));
            GL.EnableVertexAttribArray(4);
            GL.BindVertexArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        }

        public void Draw(Vector3 pixelPosition, float pixelDimensions, float time)
        {

            ModelMatrix = Matrix4.Identity;
            ModelMatrix *= Matrix4.CreateScale(pixelDimensions);
            ModelMatrix *= Matrix4.CreateRotationY(Maths.ToRadians(time * 45));
            ModelMatrix *= Matrix4.CreateRotationX(Maths.ToRadians(35));
            ModelMatrix *= Matrix4.CreateTranslation(pixelPosition);

            GlobalValues.GuiBlockShader.Use();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2DArray, GlobalValues.ArrayTexture.TextureID);
            GL.UniformMatrix4(GL.GetUniformLocation(GlobalValues.GuiBlockShader.getID(), "model"), true, ref ModelMatrix);
            GL.UniformMatrix4(GL.GetUniformLocation(GlobalValues.GuiBlockShader.getID(), "view"), true, ref GlobalValues.GuiCamera.ViewMatrix);
            GL.UniformMatrix4(GL.GetUniformLocation(GlobalValues.GuiBlockShader.getID(), "projection"), true, ref GlobalValues.GuiCamera.ProjectionMatrix);
            // GL.Uniform3(GL.GetUniformLocation(shader.getID(), "cpos"), ref ChunkPosition);
            GL.Uniform1(GL.GetUniformLocation(GlobalValues.GuiBlockShader.getID(), "time"), (float) GlobalValues.Time);
            GL.BindVertexArray(Vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, Vertices.Length);
            GL.BindVertexArray(0);

            GlobalValues.GuiBlockShader.UnUse();

        }

    }
}
