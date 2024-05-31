using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK.ChunkUtil
{
    public struct ModelVertex
    {

        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public ModelVertex(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
        {

            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;

        }

    }

    internal class Sun
    {

        public Matrix4 ModelMatrix;

        public Matrix4 TranslationMatrix = Matrix4.CreateTranslation(0,0,0);
        public Matrix4 RotationMatrix = Matrix4.CreateFromQuaternion(Quaternion.Identity);
        public Matrix4 ScaleMatrix;
        public Texture SunTexture;


        public float RadiusFromCamera;
        int Vbo, Vao;

        ModelVertex[] SunVertices =
        {

            new ModelVertex((0.5f,1f,0.5f), (0,1,0), (0,1)),
            new ModelVertex((0.5f,1f,-0.5f), (0,1,0), (0,0)),
            new ModelVertex((-0.5f,1f,-0.5f), (0,1,0), (1,0)),
            new ModelVertex((-0.5f,1f,-0.5f), (0,1,0), (1,0)),
            new ModelVertex((-0.5f,1f,0.5f), (0,1,0), (1,1)),
            new ModelVertex((0.5f,1f,0.5f), (0,1,0), (0,1))

        };

        public Sun(string textureFile, float radiusFromCamera)
        {

            RadiusFromCamera = radiusFromCamera;
            // Matrix4.CreateScale((1,RadiusFromCamera,1), out ScaleMatrix);
            ScaleMatrix = Matrix4.CreateScale(1, RadiusFromCamera, 1);
            SunTexture = new Texture(textureFile);

            Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, SunVertices.Length * Marshal.SizeOf<ChunkVertex>(), SunVertices, BufferUsageHint.DynamicDraw);

            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<ModelVertex>(), Marshal.OffsetOf<ModelVertex>(nameof(ModelVertex.Position)));
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<ModelVertex>(), Marshal.OffsetOf<ModelVertex>(nameof(ModelVertex.Normal)));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<ModelVertex>(), Marshal.OffsetOf<ModelVertex>(nameof(ModelVertex.TextureCoordinate)));
            GL.EnableVertexAttribArray(2);
            GL.BindVertexArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        }

        public void SetRotation(Vector3 eulerRotation)
        {

            Matrix4.CreateFromQuaternion(new Quaternion(eulerRotation), out RotationMatrix);

        }

        public void SetPosition(Vector3 position)
        {

            Matrix4.CreateTranslation(position, out TranslationMatrix);

        }

        public void Draw(Camera camera)
        {

            SetPosition(camera.position);

            ModelMatrix = ScaleMatrix * RotationMatrix * TranslationMatrix;
            // shader.Use();
            Globals.DefaultShader.Use();
            GL.Uniform1(GL.GetUniformLocation(Globals.DefaultShader.getID(), "sunTexture"), 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, SunTexture.getID());
            GL.UniformMatrix4(GL.GetUniformLocation(Globals.DefaultShader.getID(), "model"), true, ref ModelMatrix);
            // GL.UniformMatrix4(GL.GetUniformLocation(Globals.DefaultShader.getID(), "rotation"), true, ref r);
            GL.UniformMatrix4(GL.GetUniformLocation(Globals.DefaultShader.getID(), "view"), true, ref camera.view);
            GL.UniformMatrix4(GL.GetUniformLocation(Globals.DefaultShader.getID(), "projection"), true, ref camera.projection);
            GL.Uniform3(GL.GetUniformLocation(Globals.DefaultShader.getID(), "cameraPosition"), ref camera.position);
            // GL.Uniform3(GL.GetUniformLocation(shader.getID(), "cpos"), ref ChunkPosition);
            // GL.Uniform1(GL.GetUniformLocation(shader.getID(), "time"), (float)time);
            GL.BindTexture(TextureTarget.Texture2D, SunTexture.getID());
            GL.BindVertexArray(Vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, SunVertices.Length);
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            Globals.DefaultShader.UnUse();

        }

    }
}
