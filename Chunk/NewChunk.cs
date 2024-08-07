﻿using Blockgame_OpenTK.Util;
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
using System.Threading.Tasks.Dataflow;
using System.Drawing;

namespace Blockgame_OpenTK.ChunkUtil
{
    
    internal class NewChunk
    {

        public ushort[,,] BlockData = new ushort[Globals.ChunkSize, Globals.ChunkSize, Globals.ChunkSize];
        public ChunkVertex[] ChunkMesh;
        public GenerationState GenerationState;// = GenerationState.NotGenerated;
        public MeshState MeshState;// = MeshState.NotMeshed;
        public ChunkState ChunkState;// = ChunkState.NotReady;
        public Vector3i ChunkPosition;
        public int Vao, Vbo;

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
            try
            {

                GL.DrawArrays(PrimitiveType.Triangles, 0, ChunkMesh.Length);

            } catch
            {

                Console.WriteLine("Some reason couldn't draw. Skipping");

            }
            GL.BindVertexArray(0);

            Globals.ChunkShader.UnUse();

        }

        public Block GetBlock(Vector3i position)
        {

            return Globals.Register.GetBlockFromID(BlockData[position.X, position.Y, position.Z]);

        }

        public void SetBlock(Vector3i position, Block block)
        {

            BlockData[position.X, position.Y, position.Z] = (ushort) Globals.Register.GetIDFromBlock(block);

        }

        public Block GetBlockSafe(Vector3i position)
        {

            Vector3i clampedPosition = Vector3i.Clamp(position, (0, 0, 0), (Globals.ChunkSize, Globals.ChunkSize, Globals.ChunkSize));

            return Globals.Register.GetBlockFromID(BlockData[clampedPosition.X, clampedPosition.Y, clampedPosition.Z]);

        }
        public void SetBlockSafe(Vector3i position, Block block)
        {

            Vector3i clampedPosition = Vector3i.Clamp(position, (0, 0, 0), (Globals.ChunkSize, Globals.ChunkSize, Globals.ChunkSize));

            BlockData[clampedPosition.X, clampedPosition.Y, clampedPosition.Z] = (ushort)Globals.Register.GetIDFromBlock(block);

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

        public void SetBlockDataGlobal(Vector3i position, ushort data)
        {

            int minX = (ChunkPosition.X * Globals.ChunkSize);
            int minY = (ChunkPosition.Y * Globals.ChunkSize);
            int minZ = (ChunkPosition.Z * Globals.ChunkSize);

            int maxX = ((1 + ChunkPosition.X) * Globals.ChunkSize) - 1;
            int maxY = ((1 + ChunkPosition.Y) * Globals.ChunkSize) - 1;
            int maxZ = ((1 + ChunkPosition.Z) * Globals.ChunkSize) - 1;

            if (position.X >= minX && position.X <= maxX && position.Y >= minY && position.Y <= maxY && position.Z >= minZ && position.Z <= maxZ)
            {

                // int xValue = x % (size-1);
                // int yValue = y % (size-1);
                // /int zValue = z % (size-1);

                int xValue = position.X - (ChunkPosition.X * Globals.ChunkSize);
                int yValue = position.Y - (ChunkPosition.Y * Globals.ChunkSize);
                int zValue = position.Z - (ChunkPosition.Z * Globals.ChunkSize);

                SetBlockData((xValue, yValue, zValue), data);

            }

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
