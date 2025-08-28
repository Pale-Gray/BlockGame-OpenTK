using OpenTK.Graphics.OpenGL;

namespace VoxelGame.Rendering;

public class DeferredFramebuffer
{
    private int _framebuffer;
    private int _depthTexture;
    private int _albedoTexture;
    private int _transparentAlbedoTexture;
    private int _normalTexture;
    private FramebufferQuad _quad;

    public void Create()
    {
        _framebuffer = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);

        _albedoTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2d, _albedoTexture);
        GL.TexStorage2D(TextureTarget.Texture2d, 1, SizedInternalFormat.Rgba8, Config.Width, Config.Height);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
        GL.ObjectLabel(ObjectIdentifier.Texture, (uint)_albedoTexture, -1, "albedo");
        
        _normalTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2d, _normalTexture);
        GL.TexStorage2D(TextureTarget.Texture2d, 1, SizedInternalFormat.Rgba16f, Config.Width, Config.Height);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
        GL.ObjectLabel(ObjectIdentifier.Texture, (uint)_normalTexture, -1, "normal");
        
        _depthTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2d, _depthTexture);
        GL.TexStorage2D(TextureTarget.Texture2d, 1, SizedInternalFormat.Depth24Stencil8, Config.Width, Config.Height);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
        GL.ObjectLabel(ObjectIdentifier.Texture, (uint)_depthTexture, -1, "depth");
        
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, _albedoTexture, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2d, _normalTexture, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2d, _depthTexture, 0);
        
        GL.DrawBuffers(2, [DrawBufferMode.ColorAttachment0, DrawBufferMode.ColorAttachment1]);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        _quad = new FramebufferQuad();
        _quad.Create();
    }

    public void Bind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
    }

    public void BindTextures()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2d, _albedoTexture);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2d, _normalTexture);
        GL.ActiveTexture(TextureUnit.Texture2);
        GL.BindTexture(TextureTarget.Texture2d, _depthTexture);
    }

    public void Unbind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Draw() => _quad.Draw(this);

    public void Destroy()
    {
        GL.DeleteTexture(_albedoTexture);
        GL.DeleteTexture(_depthTexture);
        GL.DeleteTexture(_normalTexture);
        GL.DeleteTexture(_transparentAlbedoTexture);
        GL.DeleteFramebuffer(_framebuffer);
    }

    public void Resize()
    {
        Destroy();
        Create();
    }
}