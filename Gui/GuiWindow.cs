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

        public GuiWindow()
        {



        }
    }
}
