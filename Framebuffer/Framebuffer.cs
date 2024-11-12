using OpenTK.Graphics.OpenGL;
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
            GL.BindTexture(TextureTarget.Texture2d, textureColorBufferId);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba32f, (int)GlobalValues.WIDTH, (int)GlobalValues.HEIGHT, 0, PixelFormat.Rgba, PixelType.Float, (IntPtr)null);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, textureColorBufferId, 0);
            // GL.BindTexture(TextureTarget.Texture2d, 0);

            // GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, textureColorBufferId, 0);
            textureDepthStencilBufferId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2d, textureDepthStencilBufferId);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Depth24Stencil8, (int)GlobalValues.WIDTH, (int)GlobalValues.HEIGHT, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, (IntPtr)null);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2d, textureDepthStencilBufferId, 0); ;
            // GL.BindTexture(TextureTarget.Texture2d, 0);

            // GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2d, textureDepthStencilBufferId, 0);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferStatus.FramebufferComplete)
            {

                Debugger.Log("Something went wrong creating the Framebuffer.", Severity.Error);

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
            GL.DeleteTexture(textureColorBufferId);
            GL.DeleteTexture(textureDepthStencilBufferId);

        }

        public void UpdateAspect()
        {

            Destroy();

            id = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);

            textureColorBufferId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2d, textureColorBufferId);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            //GL.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            //GL.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, (int)GlobalValues.WIDTH, (int)GlobalValues.HEIGHT, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)null);
            GL.BindTexture(TextureTarget.Texture2d, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, textureColorBufferId, 0);

            textureDepthStencilBufferId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2d, textureDepthStencilBufferId);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            //GL.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            //GL.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Depth24Stencil8, (int)GlobalValues.WIDTH, (int)GlobalValues.HEIGHT, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, (IntPtr)null);
            GL.BindTexture(TextureTarget.Texture2d, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2d, textureDepthStencilBufferId, 0);

            // Why am I not using renderbuffers? Because I feel like it. Also you can view the depth texture in the fragment shader

            // renderBuffer = GL.GenRenderbuffer();
            // GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderBuffer);
            // GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, (int)Constants.WIDTH, (int)Constants.HEIGHT);
            // GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, renderBuffer);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferStatus.FramebufferComplete)
            {

                Debugger.Log("Something went wrong creating the Framebuffer.", Severity.Error);

            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        }

    }
}
