using Blockgame_OpenTK.Font;
using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Blockgame_OpenTK.Gui
{
    public class GuiRandomTextDisplay : GuiElement
    {

        public override void AddElement(GuiElement element)
        {
            throw new Exception("AddElement is not supported with GuiRandomTextDisplay");
        }

        private string _validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                     "abcdefghijklmnopqrstuvwxyz" +
                                     "1234567890_,./?!@#$%^&*()-+=[]{};:\"\' ";



        private string _text;
        public string Text { get { return _text; } set { _text = value; UpdateInitialString(); } }
        private StringBuilder _randomString = new StringBuilder();

        public float TimeInSeconds = 0.0f;
        public float TickDelayInSeconds = 0.0f;

        private float _elapsedTickTime = 0.0f;
        private float _elapsedTime = 0.0f;

        public void UpdateInitialString()
        {

            _randomString.Clear();
            foreach (char c in _text)
            {

                _randomString.Append(_validChars[new Random().Next(0, _validChars.Length)]);

            }

        }

        public void Update()
        {

            if (_randomString.ToString() != Text)
            {

                if (_elapsedTime >= TimeInSeconds)
                {

                    int index = new Random().Next(0, Text.Length);
                    if (_randomString[index] != Text[index])
                    {

                        SetRandomChar(index);

                    }

                }
                else
                {

                    SetRandomChar(new Random().Next(0, Text.Length));

                }

            }

        }

        public void SetRandomChar(int index)
        {

            _randomString[index] = _validChars[new Random().Next(0, _validChars.Length)];

        }

        public override void Draw()
        {

            if (_elapsedTickTime >= TickDelayInSeconds)
            {

                _elapsedTickTime -= TickDelayInSeconds;
                Update();

            }
            _elapsedTickTime += (float) GlobalValues.DeltaTime;
            _elapsedTime += (float) GlobalValues.DeltaTime;

            // base.Draw();
            // Dimensions = AbsoluteDimensions + GuiMath.RelativeToAbsolute(RelativeDimensions, Parent?.Dimensions ?? (GlobalValues.WIDTH, GlobalValues.HEIGHT));
            // Position = ((Parent?.Position - (Parent?.Dimensions * Parent?.Origin)) ?? (0, 0)) + AbsolutePosition + GuiMath.RelativeToAbsolute(RelativePosition, (Parent?.Dimensions) ?? (GlobalValues.WIDTH, GlobalValues.HEIGHT));
            CalculateDimensionsAndPosition();
            CachedFontRenderer.RenderFont(out (Vector2, float, float) curParam, Position, Origin, 24, (int) Dimensions.Y, "Hello World", Color);

        }

    }
}
