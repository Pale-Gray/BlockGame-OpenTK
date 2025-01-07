using Blockgame_OpenTK.Font;
using OpenTK.Core.Native;
using OpenTK.Mathematics;
using System;
using System.Formats.Tar;
using System.Linq;

namespace Blockgame_OpenTK.Gui
{
    internal class GuiTextbox : GuiElement
    {

        public string Text = "";
        public int TextSize = 16;
        public GuiElement cursorElement = new GuiElement();
        public Color4<Rgba> TextColor = Color4.Black;

        public GuiTextbox()
        {

            cursorElement.Origin = (0, 1);
            cursorElement.IsVisible = true;
            cursorElement.Layer = Layer + 2;
            cursorElement.Color = TextColor;

            TransitionElement cursorBlink = new TransitionElement(cursorElement);
            cursorBlink.ShouldLoop = true;
            cursorBlink.PlaybackMode = PlaybackMode.Forward;
            cursorBlink.IsPingPong = true;
            cursorBlink.Length = 0.25f;

            cursorBlink.StartingColor = Color4.Black;
            cursorBlink.EndingColor = new Color4<Rgba>(0, 0, 0, 0);
            cursorBlink.IsRunning = true;
            // cursorElement.IsVisible = true;

        }

        Vector2 previousCursorPos = Vector2.PositiveInfinity;
        int idxOffset = 0;
        public override void Draw()
        {

            base.Draw();

            // Console.WriteLine(Position);

            if (Input.CurrentTypedChars.Count > 0)
            {

                foreach (char c in Input.CurrentTypedChars)
                {

                    if (char.IsControl(c))
                    {

                        if (c != '\b')
                        {

                            if (Input.IsKeyDown(OpenTK.Platform.Key.Backspace))
                            {

                                for (int i = Text.Length - 1; i >= 0; i--)
                                {

                                    if (i == 0 || (Text[i - 1] == ' ' && Text[i] != ' '))
                                    {

                                        Text = Text.Remove(i, Text.Length - i);
                                        break;

                                    }

                                }
                                break;

                            }

                        }

                    }

                    switch (c)
                    {

                        case '\e':
                            break;
                        case '\b':
                            if (Text.Length > 0)
                            {
                                Text = Text.Remove(Text.Length - 1);
                            }
                            break;
                        case '\t':
                            Text += "    ";
                            break;
                        default:
                            Text += c;
                            break;

                    }

                }

            }

            if (Input.IsKeyPressed(OpenTK.Platform.Key.LeftArrow)) idxOffset++;
            if (Input.IsKeyPressed(OpenTK.Platform.Key.RightArrow)) idxOffset--;

            if (Text.Length - 1 - idxOffset < 0) idxOffset++;
            if (Text.Length - 1 - idxOffset > Text.Length - 1) idxOffset--;


            CachedFontRenderer.RenderFont(out (Vector2, float, float) curParam, Position - (Dimensions * Origin) + (0, 20), (0, 0), Layer + 1, 24, Text, TextColor, Dimensions, cursorIndex: Math.Clamp(Text.Length-1-idxOffset, 0, Text.Length-1 < 0 ? 0 : Text.Length-1));
            if (previousCursorPos == Vector2.PositiveInfinity) previousCursorPos = curParam.Item1;
            cursorElement.AbsolutePosition = Vector2.Lerp(previousCursorPos, curParam.Item1, 0.1f);
            previousCursorPos = cursorElement.AbsolutePosition;
            cursorElement.AbsoluteDimensions = (curParam.Item2, curParam.Item3);

        }

    }
}
