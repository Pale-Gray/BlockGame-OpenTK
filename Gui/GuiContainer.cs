using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Gui
{
    internal class GuiContainer : GuiElement, IContainer
    {
        public void AddElement(GuiElement child)
        {

            Children.Add(child);
            child.Parent = this;
            child.Recalculate();

        }
    }
}
