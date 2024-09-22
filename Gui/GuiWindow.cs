using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Blockgame_OpenTK.Gui
{
    internal class GuiWindow : GuiElement
    {

        public enum DecorationMode
        {

            NoDecoration,
            Decorated

        }

        public List<GuiElement> WindowElements = new List<GuiElement>();

        public DecorationMode DecorMode;
        public GuiButton WindowTab;

        public GuiWindow(Vector2 dimension, DecorationMode decorationMode) : base (dimension, TopLeft)
        {

            DecorMode = decorationMode;

            if (DecorMode != DecorationMode.NoDecoration )
            {

                Console.WriteLine("creating tab");

                WindowTab = new GuiButton((dimension.X, 20), Origin);
                WindowTab.IsMoveable = true;

            }

        }

        public void AddElement(GuiElement element)
        {

            WindowElements.Add(element);

        }

        public override void Draw(float time)
        {

            base.Draw(time);

            if (DecorMode != DecorationMode.NoDecoration)
            {

                if (WindowTab.IsGrabbed)
                {

                    Vector2 mouseDelta = Input.MouseDelta;

                    AbsolutePosition += mouseDelta;

                    if (WindowElements.Count != 0)
                    {

                        foreach (GuiElement element in WindowElements)
                        {

                            element.AbsolutePosition += mouseDelta;

                        }

                    }

                }

                WindowTab.SetAbsolutePosition(Position.X, Position.Y - WindowTab.Dimensions.Y - 1);
                WindowTab.Draw(time);

            }

            if (WindowElements.Count != 0)
            {

                foreach(GuiElement guiElement in WindowElements)
                {

                    guiElement.Draw(time);

                }

            }

        }

    }
}
