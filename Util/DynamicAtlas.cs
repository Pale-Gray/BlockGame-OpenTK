using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using StbImageSharp;
using PixelType = OpenTK.Graphics.OpenGL.PixelType;

namespace VoxelGame;

public struct TextureSection
{
    public string Name;
    public Vector2i Dimensions;
    public byte[] Data;

    public TextureSection(string name, Vector2i dimensions, byte[] data)
    {
        Name = name;
        Dimensions = dimensions;
        Data = data;
    }
}

public class DynamicAtlas
{
    public int Id;
    private string _lookupDirectory;
    private Vector2i _capacity;
    private Dictionary<string, Rectangle> _textureCoordinates = new();
    public DynamicAtlas(string path)
    {
        _lookupDirectory = path;
    }

    public Vector2[] GetTextureCoordinates(string name)
    {
        return null;
    }

    public Rectangle GetTextureCoordinates(string? name, Rectangle offset)
    {
        Rectangle rect;

        if (name == null!)
        {
            rect = _textureCoordinates["missing_texture"];
        }
        else
        {
            if (_textureCoordinates.TryGetValue(name, out rect))
            {
            
            }
            else
            {
                rect = _textureCoordinates["missing_texture"];
            }
        }
        
        rect.Position += rect.Size * offset.Position;
        rect.Size *= offset.Size;
        
        return rect;
    }
    
    public DynamicAtlas Generate()
    {
        List<TextureSection> sections = new();
        int dimensionsArea = 0;
        
        foreach (string file in Directory.GetFiles(_lookupDirectory).Where(file => file.EndsWith("png")))
        {
            using (Stream imageStream = File.OpenRead(file))
            {
                ImageResult imageResult = ImageResult.FromStream(imageStream);
                sections.Add(new TextureSection(file.Substring(_lookupDirectory.Length + 1).Split('.')[0], (imageResult.Width, imageResult.Height), imageResult.Data));
                
                if (_capacity == Vector2i.Zero) _capacity = (imageResult.Width, imageResult.Height);
                dimensionsArea += imageResult.Width * imageResult.Height;
                if (dimensionsArea > _capacity.X * _capacity.Y) _capacity *= 2;
            }
        }

        Id = GL.GenTexture();
        GL.ObjectLabel(ObjectIdentifier.Texture, (uint) Id, "texture atlas".Length, "texture atlas");
        GL.BindTexture(TextureTarget.Texture2d, Id);
        GL.TexStorage2D(TextureTarget.Texture2d, 4, SizedInternalFormat.Rgba8, _capacity.X, _capacity.Y);

        Vector2i position = Vector2i.Zero;
        for (int i = 0; i < sections.Count; i++)
        {
            TextureSection section = sections[i];
            
            if (position.X + section.Dimensions.X > _capacity.X)
            {
                position.X = 0;
                position.Y += section.Dimensions.Y;
            }
            
            GL.TexSubImage2D(TextureTarget.Texture2d, 0, position.X, position.Y, section.Dimensions.X, section.Dimensions.Y, PixelFormat.Rgba, PixelType.UnsignedByte, section.Data);
            // _textureCoordinates.Add(section.Name, [(position.X / (float) _capacity.X, (position.Y + section.Dimensions.Y) / (float) _capacity.Y), (position.X / (float) _capacity.X, position.Y / (float) _capacity.Y), ((position.X + section.Dimensions.X) / (float) _capacity.X, position.Y / (float) _capacity.Y), ((position.X + section.Dimensions.X) / (float) _capacity.X, (position.Y + section.Dimensions.Y) / (float) _capacity.Y)]);
            _textureCoordinates.Add(section.Name, new Rectangle(position / (Vector2) _capacity, section.Dimensions / (Vector2) _capacity));
            position.X += section.Dimensions.Y;
        }
        
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.NearestMipmapLinear);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
        
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMaxLevel, 4);
        GL.GenerateMipmap(TextureTarget.Texture2d);
        
        return this;
    }
}