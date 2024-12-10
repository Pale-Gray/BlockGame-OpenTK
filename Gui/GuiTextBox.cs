using Blockgame_OpenTK.Font;
using FreeTypeSharp;
using OpenTK.Mathematics;
using OpenTK.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Gui
{
    internal class GuiTextBox : GuiElement
    {

        public bool IsFocused { get; private set; }
        private int _lastIndex = 0;
        public List<string> Text = new List<string>();

        public GuiTextBox()
        {

            IsFocused = false;

        }
            
        public override void Draw()
        {

            base.Draw();

            CachedFontRenderer.RenderFont(Position, Origin, Layer - 1, Math.Max(0, AbsoluteDimensions.Y - 12), string.Concat(Text.ToArray()), Path.Combine("Resources", "Fonts", "NotoSansJP-Regular.ttf"), Color3.Red);

            if (GuiMaths.DidCollideWithMousePointer(Position, AbsoluteDimensions, Origin) && Input.IsMouseButtonPressed(OpenTK.Platform.MouseButton.Button1))
            {

                IsFocused = !IsFocused;

            }

            if (Input.IsKeyDown(Key.Escape))
            {

                IsFocused = false;

            }

            if (IsFocused)
            {

                if (Input.CurrentTypedStrings.Count != 0)
                {

                    _lastIndex = Text.Count - 1;

                    if (Input.IsKeyPressed(Key.Backspace))
                    {

                        Console.WriteLine("delete key was pressed");
                        if (Text.Count != 0)
                        {

                            Text.RemoveAt(_lastIndex);

                        }

                    }
                    else
                    {

                        if (!Input.CurrentTypedStrings.Contains("\b"))
                        {

                            Text.AddRange(Input.CurrentTypedStrings);

                        }
                        _lastIndex = Text.Count - 1;

                    }

                }

            }

        }

    }   
}
