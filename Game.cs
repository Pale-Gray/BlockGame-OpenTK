using OpenTK.Graphics.OpenGL;
// using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Transactions;

using opentk_proj.chunk;
using opentk_proj.block;
using opentk_proj.util;
using System.Collections.Generic;
using System.IO;
using StbImageWriteSharp;
using opentk_proj.gui;
using opentk_proj.animator;
using opentk_proj.framebuffer;
using System.Threading;

namespace opentk_proj
{
    internal class Game : GameWindow
    {
        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title, Flags = ContextFlags.Debug }) { }

        float[] verts = {

            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, // front
             0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

            0.5f, -0.5f, -0.5f,  0.0f, 0.0f, // right
            0.5f, -0.5f, 0.5f,  1.0f, 0.0f,
            0.5f,  0.5f, 0.5f,  1.0f, 1.0f,
            0.5f,  0.5f, 0.5f,  1.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

            0.5f, -0.5f, 0.5f, 0.0f, 0.0f, // back 
            -0.5f, -0.5f, 0.5f, 1.0f, 0.0f,
            -0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            -0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 0.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 0.0f, 0.0f,

            -0.5f, -0.5f, 0.5f,  0.0f, 0.0f, // left
            -0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, 0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, 0.5f,  0.0f, 0.0f,

            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, // top
            0.5f, 0.5f, -0.5f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            -0.5f, 0.5f, 0.5f, 0.0f, 1.0f,
            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f,

            0.5f, -0.5f, -0.5f, 0.0f, 0.0f, // bottom
            -0.5f, -0.5f, -0.5f, 1.0f, 0.0f,
            -0.5f, -0.5f, 0.5f, 1.0f, 1.0f,
            -0.5f, -0.5f, 0.5f, 1.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 0.0f, 1.0f,
            0.5f, -0.5f, -0.5f, 0.0f, 0.0f

        };

        float[] xyz_verts =
        {

            0, 0, 0, 0, 0,
            1, 0, 0, 0, 0,
            0, 0, 0, 0, 0,

            0, 0, 0, 0, 0,
            0, 1, 0, 0, 0,
            0, 0, 0, 0, 0,

            0, 0, 0, 0, 0,
            0, 0, 1, 0, 0,
            0, 0, 0, 0, 0

        };

        float[] v =
        {

             -0.5f, 0.5f, 0.0f, 0.0f, 1.0f,
             -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
             0.5f, 0.5f, 0.0f, 1.0f, 1.0f,
             -0.5f, 0.5f, 0.0f, 0.0f, 1.0f

        };

        float[] skybox =
        {

            -0.5f, -0.5f, -0.5f,  0.0f, 0.5f, // front
             0.5f, -0.5f, -0.5f,  0.25f, 0.5f,
             0.5f,  0.5f, -0.5f,  0.25f, 1f,
             0.5f,  0.5f, -0.5f,  0.25f, 1f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.5f,

            0.5f, -0.5f, -0.5f,  0.25f, 0.5f, // right
            0.5f, -0.5f, 0.5f,  0.5f, 0.5f,
            0.5f,  0.5f, 0.5f,  0.5f, 1.0f,
            0.5f,  0.5f, 0.5f,  0.5f, 1.0f,
            0.5f,  0.5f, -0.5f,  0.25f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.25f, 0.5f,

            0.5f, -0.5f, 0.5f, 0.5f, 0.5f, // back 
            -0.5f, -0.5f, 0.5f, 0.75f, 0.5f,
            -0.5f, 0.5f, 0.5f, 0.75f, 1.0f,
            -0.5f, 0.5f, 0.5f, 0.75f, 1.0f,
            0.5f, 0.5f, 0.5f, 0.5f, 1.0f,
            0.5f, -0.5f, 0.5f, 0.5f, 0.5f,

            -0.5f, -0.5f, 0.5f,  0.75f, 0.5f, // left
            -0.5f, -0.5f, -0.5f,  1f, 0.5f,
            -0.5f,  0.5f, -0.5f,  1f, 1.0f,
            -0.5f,  0.5f, -0.5f,  1f, 1.0f,
            -0.5f,  0.5f, 0.5f,  0.75f, 1.0f,
            -0.5f, -0.5f, 0.5f,  0.75f, 0.5f,

            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, // top
            0.5f, 0.5f, -0.5f, 0.25f, 0.0f,
            0.5f, 0.5f, 0.5f, 0.25f, 0.5f,
            0.5f, 0.5f, 0.5f, 0.25f, 0.5f,
            -0.5f, 0.5f, 0.5f, 0.0f, 0.5f,
            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f,

            0.5f, -0.5f, -0.5f, 0.25f, 0.0f, // bottom
            -0.5f, -0.5f, -0.5f, 0.5f, 0.0f,
            -0.5f, -0.5f, 0.5f, 0.5f, 0.5f,
            -0.5f, -0.5f, 0.5f, 0.5f, 0.5f,
            0.5f, -0.5f, 0.5f, 0.25f, 0.5f,
            0.5f, -0.5f, -0.5f, 0.25f, 0.0f

        };

        Shader shader;
        Texture texture;
        Texture emtexture;
        CMTexture cmtex;

        int vbo;
        int vao;

        float speed = 15.0f;
        Vector3 cposition = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 cfront = new Vector3(0.0f, 0.0f, -1.0f);
        Vector3 cup = new Vector3(0.0f, 1.0f, 0.0f);

        float pitch; // x rotation
        float yaw; // y rotation
        float roll; // z rotation

        float sens = 0.8f;

        double time = 0;

        Vector2 lmpos = new Vector2(0.0f, 0.0f);

        bool firstmove = true;

        double delay = 0;

        Model rmodel;
        // Chunk chunk;
        Camera camera;

        Model xyz_display;
        Model hitdisplay;
        Model Skybox;

        NakedModel nakedmodel;

        BoundingBox boundingbox = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(0.5f, 0.5f, 0.5f));
        NakedModel boundmodel;
        GUIElement TestElement;
        Keyframe GUIKeyframe = new Keyframe(0, 100, 5);
        GUIClickable GUIClick;

        Ray ray = new Ray(0, 0, 0, 0, 0, 0);

        Framebuffer frameBuffer;
        FramebufferQuad framebufferQuad;

        bool debug = false;

        bool IsGrabbed = true;

        double ft = 0;
        double fs = 0;

        Chunk c;
        protected override void OnUpdateFrame(FrameEventArgs args)
        {

            base.OnUpdateFrame(args);

            Constants.Time = DeltaTime.Get();
            Constants.Mouse = MouseState;
            Constants.Keyboard = KeyboardState;

            time = GLFW.GetTime();

            delay += args.Time;
            ft += 1d / args.Time;
            fs++;

            if (delay > 1d)
            {

                Title = "fps [" + fs + "]";
                ft = 0;
                fs = 0;
                delay = 0;

            }

            MouseState mouse = MouseState;

            if (IsGrabbed)
            {

                if (firstmove)
                {

                    lmpos = new Vector2(mouse.X, mouse.Y);
                    firstmove = false;

                }
                else
                {

                    CursorState = CursorState.Grabbed;
                    float deltaX = mouse.X - lmpos.X;
                    float deltaY = mouse.Y - lmpos.Y;
                    lmpos = new Vector2(mouse.X, mouse.Y);
                    yaw += deltaX * sens;
                    pitch -= deltaY * sens;

                }

            }

            // Console.WriteLine(verts.Length);

            KeyboardState k = KeyboardState;

            if (k.IsKeyPressed(Keys.F1))
            {

                switch(debug)
                {

                    case true:
                        debug = false;
                        break;
                    case false:
                        debug = true;
                        break;

                }

            }

            if (IsGrabbed)
            {

                CursorState = CursorState.Grabbed;

            } else
            {

                CursorState = CursorState.Normal;

            }

            if (IsGrabbed)
            {

                if (k.IsKeyDown(Keys.LeftControl))
                {

                    speed = 1.0f;

                } else
                {

                    if (k.IsKeyDown(Keys.LeftShift))
                    {

                        speed = 120.0f;

                    }
                    else
                    {

                        speed = 60.0f;

                    }

                }

                if (k.IsKeyDown(Keys.W))
                {

                    // cposition += cfront * speed * (float)args.Time;
                    cposition += cfront * speed * (float)Constants.Time;

                }
                if (k.IsKeyDown(Keys.S))
                {

                    cposition -= cfront * speed * (float)args.Time;

                }
                if (k.IsKeyDown(Keys.A))
                {

                    cposition -= Vector3.Normalize(Vector3.Cross(cfront, cup)) * (speed * (float)args.Time);

                }
                if (k.IsKeyDown(Keys.D))
                {

                    cposition += Vector3.Normalize(Vector3.Cross(cfront, cup)) * (speed * (float)args.Time);

                }
                if (k.IsKeyDown(Keys.E))
                {

                    cposition += cup * speed * (float)args.Time;

                }
                if (k.IsKeyDown(Keys.Q))
                {

                    cposition -= cup * speed * (float)args.Time;

                }

            }

            if (k.IsKeyDown(Keys.Escape))
            {

                Close();

            }

            if (k.IsKeyPressed(Keys.M))
            {

                IsGrabbed = !IsGrabbed;

            }

        }
        protected override void OnLoad()
        {

            base.OnLoad();

            Thread.CurrentThread.Name = "MAIN";

            BinaryWriter bw = new BinaryWriter(File.Open("../../../res/cdat/1.cdat", FileMode.OpenOrCreate));

            bw.Write((uint)500);

            Blocks.RegisterIDs();

            // chunk = new Chunk(0, 0, 0);
            camera = new Camera(cposition, cfront, cup, CameraType.Perspective, 45.0f);
            rmodel = new Model(verts, "../../../res/textures/debug.png", "../../../res/shaders/model.vert", "../../../res/shaders/model.frag");
            hitdisplay = new Model(verts, "../../../res/textures/debug.png", "../../../res/shaders/model.vert", "../../../res/shaders/model.frag");
            xyz_display = new Model(xyz_verts, null, "../../../res/shaders/debug.vert", "../../../res/shaders/debug.frag");

            nakedmodel = new NakedModel(NakedModel.Tri);

            rmodel.SetRotation(0, 45, 0);
            rmodel.SetScale(0.1f, 0.1f, 0.1f);

            boundmodel = new NakedModel(boundingbox.triangles);

            Skybox = new Model(skybox, "../../../res/textures/cubemap/cubemap_test.png", "../../../res/shaders/model.vert", "../../../res/shaders/model.frag");

            xyz_display.SetScale(0.25f, 0.25f, 0.25f);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.TextureCubeMap);
            GL.Enable(EnableCap.TextureCubeMapSeamless);
            GL.ActiveTexture(TextureUnit.Texture0);

            texture = new Texture("../../../res/textures/atlas.png");
            emtexture = new Texture("../../../res/textures/atlas_em.png");
            Texture t = new Texture("../../../res/textures/cubemap/cubemap_test.png");
            cmtex = new CMTexture(t, 64);

            TestElement = new GUIElement(50, 50, 10, 10, OriginType.Center, t, GUIElement.Null);
            // GUIClick = new GUIClickable(50, 50, 20, 20, OriginType.Center);
            // DeltaTime.Get();

            shader = new Shader("../../../res/shaders/default.vert", "../../../res/shaders/default.frag");
            shader.Use();
            //Texture t = new Texture("../../../res/textures/cubemap/cubemap_template.png");
            //cmtex = new CMTexture(t, 64);
            // Texture t = new Texture("../../../res/textures/portiontest.png");
            // Texture.GetPortion(t, 0, 0, 32, 32);
            shader.UnUse();

            frameBuffer = new Framebuffer();
            framebufferQuad = new FramebufferQuad();

            // ChunkLoader.Append(new Chunk(0, 0, 0));
            // ChunkLoader.Append(new Chunk(1, 0, 0));

            // ChunkLoader.GenerateChunksWithinRadius(24);
            // c = new Chunk(0,0,0);
            // Size = (1920, 1080);
            // Location = (0, 0);

            // Console.WriteLine(ChunkLoader.GetChunkAtPosition(1, 0, 0).cx);
            ChunkLoader.PregenerateChunkPositionsFromRadius(6);
            // ChunkLoader.GenerateChunksWithinRadius(8);

        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {

            base.OnRenderFrame(args);

            frameBuffer.Bind();
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 transformation = Matrix4.CreateScale(1.0f);
            Matrix4 model = Matrix4.CreateScale(1.0f);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)Constants.WIDTH / (float)Constants.HEIGHT, 0.1f, 100.0f);

            cfront.X = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(yaw));
            cfront.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
            cfront.Z = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(yaw));
            cfront = Vector3.Normalize(cfront);

            Matrix4 view = Matrix4.LookAt(cposition, cposition + cfront, cup);

            camera.Update(cposition, cfront, cup, yaw, pitch, roll);
            // etc
            GL.Disable(EnableCap.DepthTest);
            // Skybox.SetRotation((float)time*5, 0, 0);
            Skybox.Draw(cposition, camera, (float)time);
            GL.Enable(EnableCap.DepthTest);

            MouseState mouse = MouseState;
            shader.Use();
            GL.Uniform1(GL.GetUniformLocation(shader.id, "tex"), 0);
            GL.Uniform1(GL.GetUniformLocation(shader.id, "emission"), 1);
            GL.Uniform1(GL.GetUniformLocation(shader.id, "cubemap"), 2);
            GL.Uniform3(GL.GetUniformLocation(shader.id, "campos"), camera.position);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture.getID());
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, emtexture.getID());
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.TextureCubeMap, cmtex.id);

            ChunkLoader.GenerateChunksWithinRadiusStaggered(6, (float)Constants.Time);
            ChunkLoader.DrawChunks(shader, camera, (float)time);
            // c.Draw(shader, camera, (float)time);
            // ChunkLoader.DrawChunksSmarter(3, 8, shader, camera, (float)time, (float)args.Time);

            shader.UnUse();

            GL.Disable(EnableCap.DepthTest);
            TestElement.Draw();
            GL.Enable(EnableCap.DepthTest);

            if (debug)
            {

                rmodel.SetScale(1.0f, 1.0f, 1.0f);
                rmodel.SetRotation(0.0f, 0.0f, 0.0f);
                rmodel.Draw(new Vector3(0, 0, 0), camera, (float)time);

                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.DepthTest);
                // GL.LineWidth(5f);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                xyz_display.Draw(cposition + (cfront * 5), camera, (float)time);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.Enable(EnableCap.CullFace);
                GL.Enable(EnableCap.DepthTest);

            }
            frameBuffer.Unbind();

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            framebufferQuad.Draw(frameBuffer, (float)time);
            KeyboardState ks = KeyboardState;
            if (ks.IsKeyPressed(Keys.F1))
            {

                StbImageWrite.stbi_flip_vertically_on_write(1);
                Stream str = File.OpenWrite("../../../res/ss/ss1.png");
                StreamReader sr = new StreamReader(File.OpenRead("../../../res/textures/testatlas.png"));
                ImageWriter wr = new ImageWriter();
                byte[] pixels = new byte[(640 * 480) * 4];
                GL.ReadPixels(0, 0, 640, 480, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
                wr.WritePng(pixels, 640, 480, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, str);

            }
            if (ks.IsKeyPressed(Keys.F5))
            {

                // chunk.Save("../../../res/cdat/chunk.cdat");
                for (int i = 0; i < ChunkLoader.GetAllChunks().Count; i++)
                {
                    string key = ChunkLoader.GetAllChunks().ElementAt(i).Key;

                    ChunkLoader.GetAllChunks()[key].Save("../../../res/cdat/"+key+".cdat");


                }

            }
            if (ks.IsKeyPressed(Keys.F6))
            {

                // chunk.Load("../../../res/cdat/chunk.cdat");

                for (int i = 0; i < ChunkLoader.GetAllChunks().Count; i++)
                {
                    string key = ChunkLoader.GetAllChunks().ElementAt(i).Key;

                    ChunkLoader.GetAllChunks()[key].Load("../../../res/cdat/" + key + ".cdat");


                }

            }
            if (ks.IsKeyPressed(Keys.F7))
            {

                // chunk.Rewrite();

                for (int i = 0; i < ChunkLoader.GetAllChunks().Count; i++)
                {
                    
                    string key = ChunkLoader.GetAllChunks().ElementAt(i).Key;

                    ChunkLoader.GetAllChunks()[key].Rewrite();


                }

            }

            SwapBuffers();

        }
        protected override void OnUnload()
        {

            base.OnUnload();

            // this portion is not required
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vbo);

            shader.Dispose();

        }
        protected override void OnResize(ResizeEventArgs e)
        {

            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
            Constants.WIDTH = e.Width;
            Constants.HEIGHT = e.Height;

            camera.UpdateProjectionMatrix();
            TestElement.Update();
            frameBuffer.UpdateAspect();
            // GUIClick.Update();

        }

    }

}
