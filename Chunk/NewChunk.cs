using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using static FastNoise;
using Blockgame_OpenTK.BlockUtil;

namespace Blockgame_OpenTK.ChunkUtil
{
    
    internal class NewChunk
    {

        ushort[,,] BlockData = new ushort[Globals.ChunkSize, Globals.ChunkSize, Globals.ChunkSize];
        ChunkVertex[] ChunkMesh;
        GenerationState GenerationState;// = GenerationState.NotGenerated;
        MeshState MeshState;// = MeshState.NotMeshed;
        ChunkState ChunkState;// = ChunkState.NotReady;
        Vector3i ChunkPosition;
        int Vao, Vbo;

        public NewChunk(Vector3i chunkPosition)
        {

            ChunkPosition = chunkPosition;
            GenerationState = GenerationState.NotGenerated;
            MeshState = MeshState.NotMeshed;
            ChunkState = ChunkState.NotReady;

        }

        public void Draw(Camera camera)
        {

            Globals.ChunkShader.Use();

            // Console.WriteLine(ChunkPosition);

            GL.Uniform1(GL.GetUniformLocation(Globals.ChunkShader.id, "atlas"), 0);
            GL.Uniform1(GL.GetUniformLocation(Globals.ChunkShader.id, "arrays"), 1);
            GL.Uniform3(GL.GetUniformLocation(Globals.ChunkShader.id, "cameraPosition"), (0, 0, 0));
            // Console.WriteLine(ChunkPosition);
            GL.Uniform3(GL.GetUniformLocation(Globals.ChunkShader.id, "chunkpos"), ChunkPosition.ToVector3());
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Globals.AtlasTexture.getID());
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2DArray, Globals.ArrayTexture.TextureID);
            // GL.UniformMatrix4(GL.GetUniformLocation(Globals.ChunkShader.getID(), "model"), true, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(Globals.ChunkShader.getID(), "view"), true, ref camera.ViewMatrix);
            GL.UniformMatrix4(GL.GetUniformLocation(Globals.ChunkShader.getID(), "projection"), true, ref camera.ProjectionMatrix);
            // GL.Uniform3(GL.GetUniformLocation(shader.getID(), "cpos"), ref ChunkPosition);
            GL.Uniform1(GL.GetUniformLocation(Globals.ChunkShader.getID(), "time"), (float)0);
            GL.BindVertexArray(Vao);
            // Console.WriteLine(ChunkMesh.Length);
            GL.DrawArrays(PrimitiveType.Triangles, 0, ChunkMesh.Length);
            GL.BindVertexArray(0);

            Globals.ChunkShader.UnUse();

        }

        public Block GetBlock(Vector3i position)
        {

            return Globals.Register.GetBlockFromID(BlockData[position.X, position.Y, position.Z]);

        }

        public ushort GetBlockID(Vector3i position)
        {

            return BlockData[position.X, position.Y, position.Z];

        }

        public GenerationState GetGenerationState()
        {

            return GenerationState;

        }

        public MeshState GetMeshState()
        {

            return MeshState;

        }

        public ChunkState GetChunkState()
        {

            return ChunkState;

        }

        public void SetGenerationState(GenerationState state)
        {


            GenerationState = state;

        }

        public void SetMeshState(MeshState state)
        {

            MeshState = state;

        }

        public void SetChunkState(ChunkState state)
        {

            ChunkState = state;

        }

        public ushort[,,] GetBlockData()
        {

            return BlockData;

        }

        public void SetBlockData(Vector3i position, ushort data)
        {

            BlockData[position.X, position.Y, position.Z] = data;

        }

        public int GetVao()
        {

            return Vao;

        }

        public void SetVao(int vao)
        {

            Vao = vao;

        }

        public int GetVbo()
        {

            return Vbo;

        }

        public void SetVbo(int vbo)
        {

            Vbo = vbo;

        }

        public ChunkVertex[] GetChunkMesh()
        {

            return ChunkMesh;

        }

        public void SetChunkMesh(ChunkVertex[] mesh)
        {

            ChunkMesh = mesh;

        }

        public Vector3i GetChunkPosition()
        {

            return ChunkPosition;

        }

        public void SetChunkPosition(Vector3i position)
        {

            ChunkPosition = position;

        }

    }

}
