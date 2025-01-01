using Blockgame_OpenTK.Util;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Blockgame_OpenTK.Gui
{
    internal class GuiElement
    {

        public Vector2 RelativePosition;
        public Vector2 AbsolutePosition;
        public Vector2 Position { get; protected set; }

        public Vector2 RelativeDimensions;
        public Vector2 AbsoluteDimensions;
        public Vector2 Dimensions { get; protected set; }

        public Vector2 Origin = (0.0f, 0.0f);
        public float Layer = 0;

        public GuiElement Parent;
        public List<GuiElement> Children = new List<GuiElement>();

        public Color4<Rgba> Color = Color4.White;

        private Texture _texture;
        public Texture Texture { get { return _texture; } set { if (_texture != null) { _texture.Dispose(); } _texture = value; } }

        private bool _isVisible = false;
        public bool IsVisible { get { return _isVisible; } set { _isVisible = value; if (_isVisible) { GuiRenderer.Elements.Add(this); } else { GuiRenderer.Elements.Remove(this); } } }
        public TransitionElement TransitionElement = null;
        public virtual void Draw()
        {

            CalculateDimensionsAndPosition();
            GuiRenderer.RenderElement(this);
            if (TransitionElement != null) TransitionElement.Update();

            if (Children.Count > 0)
            {

                foreach (GuiElement element in Children)
                {

                    element.Layer = Layer + 1;
                    element.Draw();

                }

            }

        }

        public void CalculateDimensionsAndPosition()
        {

            Dimensions = AbsoluteDimensions + GuiMath.RelativeToAbsolute(RelativeDimensions, Parent?.Dimensions ?? (GlobalValues.WIDTH, GlobalValues.HEIGHT));
            Position = ((Parent?.Position - (Parent?.Dimensions * Parent?.Origin)) ?? (0, 0)) + AbsolutePosition + GuiMath.RelativeToAbsolute(RelativePosition, (Parent?.Dimensions) ?? (GlobalValues.WIDTH, GlobalValues.HEIGHT));

        }

        public virtual void AddElement(GuiElement element)
        {

            element.Parent = this;
            Children.Add(element);

        }

    }
}
