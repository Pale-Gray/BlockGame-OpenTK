using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.Vulkan;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

namespace Blockgame_OpenTK.Gui
{
    public struct GuiVertex
    {

        public Vector2 Position;
        public Vector2 TextureCoordinates;
        public int Layer;

        public GuiVertex(Vector2 position, Vector2 textureCoordinates, int layer)
        {

            Position = position;
            TextureCoordinates = textureCoordinates;
            Layer = layer;

        }

    }

    public enum TextureMode
    {

        Stretch = 0,
        Tile = 1

    }

    internal class GuiElement
    {

        public List<GuiElement> Children = new List<GuiElement>();
        public GuiElement Parent = null;

        private bool _isVisible = false;
        public bool IsVisible
        {

            get { return _isVisible; }

            set
            {

                _isVisible = value;
                if (value)
                {

                    if (!GlobalValues.GlobalGuiElements.Contains(this))
                    {

                        GlobalValues.GlobalGuiElements.Add(this);

                    }

                } else
                {

                    if (GlobalValues.GlobalGuiElements.Contains(this))
                    {

                        GlobalValues.GlobalGuiElements.Remove(this);

                    }

                }

            }

        }

        private Vector2 _absolutePosition;
        private Vector2 _relativePosition;
        private Vector2 _origin;
        private Vector2 _dimensions;

        private Vector2 _previousPosition;
        private Vector2 _previousDimensions;
        private Vector2 _previousOrigin;

        private Texture _texture;
        private string _textureName;
        private int _vao;
        private int _vbo;
        private GuiVertex[] _guiVertices;

        public Vector2 AbsolutePosition { get { return _absolutePosition; } set { _absolutePosition = value; } }
        public Vector2 RelativePosition { get { return _relativePosition; } set { _relativePosition = value; } }
        public Vector2 Position { get; private set; }
        public Vector2 Dimensions { get { return _dimensions; } set { _dimensions = value; } }
        public Vector2 Origin { get { return _origin; } set { _origin = value; } }

        public float? TileSize;
        public int Layer;
        private TextureMode _textureMode = TextureMode.Stretch;
        public TextureMode TextureMode { get { return _textureMode; } set { _textureMode = value; GenerateMesh(); } }
        public string TextureName { get { return _textureName; } set { _textureName = value; if (_texture != null) _texture.Dispose(); _texture = new Texture(Path.Combine(GlobalValues.GuiTexturePath, value)); } }
        public Color3<Rgb> Color = Color3.White;
        public GuiElement() { }

        private void CalculatePosition()
        {

            if (Parent == null)
            {

                Position = AbsolutePosition + GuiMath.RelativeToAbsolute(RelativePosition, (GlobalValues.WIDTH, GlobalValues.HEIGHT));

            }
            else
            {

                Position = (Parent.Position - (Parent.Dimensions * Parent.Origin)) + AbsolutePosition + GuiMath.RelativeToAbsolute(RelativePosition, Parent.Dimensions);

            }

        }
        private void GenerateMesh()
        {

            CalculatePosition();

            if (_texture == null) _texture = new Texture(Path.Combine(GlobalValues.GuiTexturePath, "Blank.png"));

            if (TextureMode == TextureMode.Stretch)
            {

                _guiVertices = new GuiVertex[]
                {

                    new GuiVertex(Position, (0, 1), 0),
                    new GuiVertex(Position + (0, Dimensions.Y), (0, 0), 0),
                    new GuiVertex(Position + Dimensions, (1, 0), 0),
                    new GuiVertex(Position + Dimensions, (1, 0), 0),
                    new GuiVertex(Position + (Dimensions.X, 0), (1, 1), 0),
                    new GuiVertex(Position, (0, 1), 0)

                };

            } else
            {

                Vector2 textureScale = _dimensions / TileSize ?? (_texture.Width, _texture.Height);
                // textureScale += textureScale * _origin;

                _guiVertices = new GuiVertex[]
                {

                    new GuiVertex(Position, (0, 1), 0),
                    new GuiVertex(Position + (0, Dimensions.Y), (0, 1 - textureScale.Y), 0),
                    new GuiVertex(Position + Dimensions, (textureScale.X, 1 - textureScale.Y), 0),
                    new GuiVertex(Position + Dimensions, (textureScale.X, 1 - textureScale.Y), 0),
                    new GuiVertex(Position + (Dimensions.X, 0), (textureScale.X, 1), 0),
                    new GuiVertex(Position, (0, 1), 0)

                };

            }

            for (int i = 0; i < _guiVertices.Length; i++)
            {

                _guiVertices[i].Position -= (Dimensions * Origin);

            }

            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);

            // _vao = GL.GenVertexArray();
            // _vbo = GL.GenBuffer();

            _vao = GL.CreateVertexArray();
            _vbo = GL.CreateBuffer();

            GL.NamedBufferStorage(_vbo, _guiVertices.Length * Marshal.SizeOf<GuiVertex>(), _guiVertices, BufferStorageMask.DynamicStorageBit);

            GL.VertexArrayVertexBuffer(_vao, 0, _vbo, 0, Marshal.SizeOf<GuiVertex>());

            GL.EnableVertexArrayAttrib(_vao, 0);
            GL.EnableVertexArrayAttrib(_vao, 1);
            GL.EnableVertexArrayAttrib(_vao, 2);

            GL.VertexArrayAttribFormat(_vao, 0, 2, VertexAttribType.Float, false, (uint) Marshal.OffsetOf<GuiVertex>(nameof(GuiVertex.Position)));
            GL.VertexArrayAttribFormat(_vao, 1, 2, VertexAttribType.Float, false, (uint)Marshal.OffsetOf<GuiVertex>(nameof(GuiVertex.TextureCoordinates)));
            GL.VertexArrayAttribFormat(_vao, 2, 1, VertexAttribType.Int, false, (uint)Marshal.OffsetOf<GuiVertex>(nameof(GuiVertex.Layer)));

            GL.VertexArrayAttribBinding(_vao, 0, 0);
            GL.VertexArrayAttribBinding(_vao, 1, 0);
            GL.VertexArrayAttribBinding(_vao, 2, 0);

        }

        public virtual void Draw()
        {

            if (_previousPosition != Position)
            {

                _previousPosition = Position;
                Recalculate();

            }
            if (_previousDimensions != Dimensions)
            {

                _previousDimensions = Dimensions;
                Recalculate();

            }
            if (_previousOrigin != Origin)
            {

                _previousOrigin = Origin;
                Recalculate();

            }

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2d, _texture.GetID());
            GlobalValues.GuiShader.Use();
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.GuiShader.id, "view"), 1, true, GlobalValues.GuiCamera.ViewMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(GlobalValues.GuiShader.id, "projection"), 1, true, GlobalValues.GuiCamera.ProjectionMatrix);
            GL.Uniform1f(GL.GetUniformLocation(GlobalValues.GuiShader.id, "guiTexture"), 0);
            GL.Uniform2f(GL.GetUniformLocation(GlobalValues.GuiShader.id, "guiSize"), 1, _dimensions);
            GL.Uniform1i(GL.GetUniformLocation(GlobalValues.GuiShader.id, "textureMode"), 1, (int) TextureMode);
            GL.Uniform3f(GL.GetUniformLocation(GlobalValues.GuiShader.id, "guiColor"), 1, (Vector3) Color);
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _guiVertices.Length);

            if (Children.Count > 0)
            {

                // Console.WriteLine("yes");
                foreach (GuiElement child in Children) child.Draw();

            }

        }

        public void Recalculate()
        {

            GenerateMesh();
            if (Children.Count > 0) { foreach (GuiElement child in Children) { child.Recalculate(); } }

        }

    }
}
