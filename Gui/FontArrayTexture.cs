using System.Collections.Generic;
using FreeTypeSharp;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Blockgame_OpenTK.Core.Gui;

public class FontArrayTexture
{

    Dictionary<char, int> _charIndex = new();

    public int TextureId;
    public Vector2i Size;

    private int _currentLayer = 0;

    public FontArrayTexture(int width, int height)
    {

        TextureId = GL.CreateTexture(TextureTarget.Texture2dArray);
        GL.TextureParameteri(TextureId, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TextureParameteri(TextureId, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TextureParameteri(TextureId, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TextureParameteri(TextureId, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
        GL.TextureStorage3D(TextureId, 1, SizedInternalFormat.R8, width, height, GL.GetInteger(GetPName.MaxArrayTextureLayers));
        Size = (width, height);

    }

    public int GetTextureIndex(char character)
    {

        return _charIndex[character];

    }

    public unsafe void AddGlyph(char character, FT_Bitmap_ glyphBitmap)
    {

        GL.PixelStorei(PixelStoreParameter.UnpackAlignment, 1);
        GL.TextureSubImage3D(TextureId, 0, 0, 0, _currentLayer, (int) glyphBitmap.width, (int) glyphBitmap.rows, 1, PixelFormat.Red, PixelType.UnsignedByte, glyphBitmap.buffer);
        _charIndex.Add(character, _currentLayer);
        _currentLayer++;

    }

    public void Free()
    {

        GL.DeleteTexture(TextureId);
        _charIndex.Clear();

    }

}