using Blockgame_OpenTK.BlockUtil;
using Blockgame_OpenTK.Core.Chunks;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
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
        public GuiBlockModel(BlockModel block)
        {

            List<ChunkVertex> vertices = new List<ChunkVertex>();

            // Console.Log(block.BlockModel == null);

            if (block != null)
            {

                if (block.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Up)) vertices.AddRange(block.ChunkReadableFaces[BlockModelCullDirection.Up]);
                if (block.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Down)) vertices.AddRange(block.ChunkReadableFaces[BlockModelCullDirection.Down]);
                if (block.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Left)) vertices.AddRange(block.ChunkReadableFaces[BlockModelCullDirection.Left]);
                if (block.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Right)) vertices.AddRange(block.ChunkReadableFaces[BlockModelCullDirection.Right]);
                if (block.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Back)) vertices.AddRange(block.ChunkReadableFaces[BlockModelCullDirection.Back]);
                if (block.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.Front)) vertices.AddRange(block.ChunkReadableFaces[BlockModelCullDirection.Front]);
                if (block.ChunkReadableFaces.ContainsKey(BlockModelCullDirection.None)) vertices.AddRange(block.ChunkReadableFaces[BlockModelCullDirection.None]);

            }

            Vertices = ToGuiCoordinateSystem(vertices.ToArray());

            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);
            Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * Marshal.SizeOf<ChunkVertex>(), Vertices, BufferUsage.StaticDraw);

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

        private ChunkVertex[] ToGuiCoordinateSystem(ChunkVertex[] face)
        {

            List<ChunkVertex> vertices = new List<ChunkVertex>();

            for (int i = 0; i < face.Length; i++)
            {

                ChunkVertex vert = face[i];
                vert.Position -= (0.5f, 0.5f, 0.5f);
                vert.Position *= -1;
                vert.Normal *= -1;

                vertices.Add(vert);

            }

            return vertices.ToArray();

        }

        public void Draw(Vector3 pixelPosition, float pixelDimensions, float time)
        {

            ModelMatrix = Matrix4.Identity;
            ModelMatrix *= Matrix4.CreateScale(pixelDimensions);
            ModelMatrix *= Matrix4.CreateRotationY(Maths.ToRadians(-time * 45));
            ModelMatrix *= Matrix4.CreateRotationX(Maths.ToRadians(-35));
            ModelMatrix *= Matrix4.CreateTranslation(pixelPosition);

            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GlobalValues.GuiBlockShader.Use();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2dArray, GlobalValues.ArrayTexture.TextureID);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.GuiBlockShader.getID(), "model"), 1, true, ModelMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.GuiBlockShader.getID(), "view"), 1, true, GlobalValues.GuiCamera.ViewMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.GuiBlockShader.getID(), "projection"), 1, true, GlobalValues.GuiCamera.ProjectionMatrix);
            // GL.Uniform3(GL.GetUniformLocation(shader.getID(), "cpos"), ref ChunkPosition);
            GL.Uniform1f(GL.GetUniformLocation(GlobalValues.GuiBlockShader.getID(), "time"), (float) GlobalValues.Time);
            GL.BindVertexArray(Vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, Vertices.Length);
            // GL.BindVertexArray(0);

            // GlobalValues.GuiBlockShader.UnUse();
            GL.Enable(EnableCap.CullFace);

        }

    }
}
