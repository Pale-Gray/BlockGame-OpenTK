using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using OpenTK.Platform;
using System;

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

                Vector2 mouseDelta = Input.MouseDelta;

                AbsolutePosition += Input.MouseDelta;

            }

            if (!Input.IsMouseButtonDown(MouseButton.Button1) && GuiMaths.DidCollideWithMousePointer(Position, Dimensions, Origin))
            {

                OnButtonHover();

            }

            if (Input.IsMouseButtonDown(MouseButton.Button1) && GuiMaths.DidCollideWithMousePointer(Position, Dimensions, Origin))
            {

                IsGrabbed = true;

                // OnButtonHold();

            }

            if (Input.IsMouseButtonDown(MouseButton.Button1) && GuiMaths.DidCollideWithMousePointer(Position, Dimensions, Origin))
            {

                IsGrabbed = false;

                // OnButtonUnclick();

            }

            if (Input.IsMouseButtonDown(MouseButton.Button1) && GuiMaths.DidCollideWithMousePointer(Position, Dimensions, Origin))
            {

                OnButtonClick();

            }

        }

    }
}
