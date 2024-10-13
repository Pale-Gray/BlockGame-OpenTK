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

        public GuiButton()
        {

            

        }

    }
}
