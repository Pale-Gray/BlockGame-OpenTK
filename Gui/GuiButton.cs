﻿using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Transactions;

namespace Blockgame_OpenTK.Gui
{
    internal class GuiButton : GuiElement
    {

        public bool IsMoveable = false;
        public bool IsGrabbed = false;

        public static Vector3 ColorTint = (1, 1, 1);

        public delegate void ButtonCallback();

        public ButtonCallback OnButtonClick = () => { ColorTint *= 0.9f;  Console.WriteLine("I was clicked!"); };
        public ButtonCallback OnButtonUnclick;
        public ButtonCallback OnButtonHold;
        public ButtonCallback OnButtonHover = () => { ColorTint *= 0.9f; Console.WriteLine("I'm being hovered over!"); };

        public GuiButton(Vector2 dimensions, Vector2 origin) : base(dimensions, origin)
        {

            

        }

        public override void Draw(float time)
        {

            base.Draw(0);

            if (IsMoveable && IsGrabbed)
            {

                Vector2 mouseDelta = Globals.Mouse.Delta;

                AbsolutePosition += Globals.Mouse.Delta;

            }

            if (!Globals.Mouse.IsButtonDown(MouseButton.Left) && GuiMaths.DidCollideWithMousePointer(Position, Dimensions, Origin))
            {

                OnButtonHover();

            }

            if (Globals.Mouse.IsButtonDown(MouseButton.Left) && GuiMaths.DidCollideWithMousePointer(Position, Dimensions, Origin))
            {

                IsGrabbed = true;

                // OnButtonHold();

            }

            if (Globals.Mouse.IsButtonReleased(MouseButton.Left) && GuiMaths.DidCollideWithMousePointer(Position, Dimensions, Origin))
            {

                IsGrabbed = false;

                // OnButtonUnclick();

            }

            if (Globals.Mouse.IsButtonPressed(MouseButton.Left) && GuiMaths.DidCollideWithMousePointer(Position, Dimensions, Origin))
            {

                OnButtonClick();

            }

        }

    }
}