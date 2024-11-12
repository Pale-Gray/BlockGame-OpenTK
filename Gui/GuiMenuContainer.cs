using Blockgame_OpenTK.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Gui
{
    internal class GuiMenuContainer
    {

        public Dictionary<string, GuiContainer> MenuLayers { get; private set; }
        private string _currentMenuLayer = "first";
        public string CurrentMenuLayer { get { return _currentMenuLayer; } set { if (MenuLayers.ContainsKey(value)) _currentMenuLayer = value; } }

        public GuiMenuContainer()
        {

            MenuLayers = new Dictionary<string, GuiContainer> { { "first", new GuiContainer() } };

        }
        public void Draw()
        {

            MenuLayers[CurrentMenuLayer].Draw();

        }

    }
}
