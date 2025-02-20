using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Resources;

namespace Blockgame_OpenTK.Font
{
    public class TextFormatter
    {

        struct GlyphData
        {

            public Color3<Rgb> Color;
            public bool IsItalic;
            public bool ShouldMove;

        }

        struct CharSyntaxInfo
        {

            public int StartIndex;
            public int Length;
            public string Text;
            public SyntaxType[] SyntaxTypes;

        }

        enum SyntaxType
        {

            Color,
            Property,
            None,
            End

        }
        public static string FormatText(string text, Color3<Rgb> defaultColor)
        {

            List<CharSyntaxInfo> info = new List<CharSyntaxInfo>();
            for (int i = 0; i < text.Length; i++)
            {



            }
            return text;

        }

        private static SyntaxType[] ParseSyntax(string syntax)
        {

            Console.WriteLine(syntax);
            string[] syntaxes = syntax.Split(';');
            List<SyntaxType> parsedTypes = new List<SyntaxType>();
            for (int i = 0; i < syntaxes.Length; i++)
            {

                if (syntaxes[i].StartsWith("0x"))
                {

                    parsedTypes.Add(SyntaxType.Color);

                } else if (syntaxes[i] == "italic" || syntaxes[i] == "wiggle")
                {

                    parsedTypes.Add(SyntaxType.Property);

                } else
                {

                    parsedTypes.Add(SyntaxType.None);

                }

            }

            return parsedTypes.ToArray();

        }

    }
}
