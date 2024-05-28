using OpenTK.Graphics.OpenGL4;
using System;

using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK.FramebufferUtil
{
    internal class Framebuffer
    {
        public int id;
        public int textureColorBufferId;
        public int textureDepthStencilBufferId;
        public int renderBuffer;
        public Framebuffer()
        {

            id = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);

            textureColorBufferId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureColorBufferId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)Globals.WIDTH, (int)Globals.HEIGHT, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)null);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, textureColorBufferId, 0);

            textureDepthStencilBufferId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureDepthStencilBufferId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, (int)Globals.WIDTH, (int)Globals.HEIGHT, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, (IntPtr)null);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2D, textureDepthStencilBufferId, 0);

            // Why am I not using renderbuffers? Because I feel like it. Also you can view the depth texture in the fragment shader

            // renderBuffer = GL.GenRenderbuffer();
            // GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderBuffer);
            // GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, (int)Constants.WIDTH, (int)Constants.HEIGHT);
            // GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, renderBuffer);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {

                DebugMessage.WriteLine("Something went wrong creating the Framebuffer.", DebugMessageType.Error);

            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        }

        public void Bind()
        {

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);

        }

        public void Unbind()
        {

            GL.BindFramebuffer (FramebufferTarget.Framebuffer, 0);

        }

        public void Destroy()
        {

            GL.DeleteFramebuffer(id);

        }

        public void UpdateAspect()
        {

            Destroy();

            id = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);

            textureColorBufferId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureColorBufferId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)Globals.WIDTH, (int)Globals.HEIGHT, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)null);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, textureColorBufferId, 0);

            textureDepthStencilBufferId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureDepthStencilBufferId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, (int)Globals.WIDTH, (int)Globals.HEIGHT, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, (IntPtr)null);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2D, textureDepthStencilBufferId, 0);

            // Why am I not using renderbuffers? Because I feel like it. Also you can view the depth texture in the fragment shader

            // renderBuffer = GL.GenRenderbuffer();
            // GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderBuffer);
            // GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, (int)Constants.WIDTH, (int)Constants.HEIGHT);
            // GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, renderBuffer);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {

                DebugMessage.WriteLine("Something went wrong creating the Framebuffer.", DebugMessageType.Error);

            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        }

    }
}
