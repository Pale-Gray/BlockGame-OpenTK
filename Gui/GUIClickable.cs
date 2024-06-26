﻿using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

using Blockgame_OpenTK.Util;

namespace Blockgame_OpenTK.Gui
{
    internal class GUIClickable : GUIElement
    {

        GUIBoundingBox BoundingBox;
        Vector2 MouseDeltaPositionOffset;
        public GUIClickable(float x, float y, float w, float h, OriginType originType, Texture texture, Vector4i texCoords) : base(x, y, w, h, originType, texture, texCoords)
        {

            BoundingBox = new GUIBoundingBox(this, originType);

        }

        public new void Draw() 
        {

            base.Draw();

            // Console.WriteLine(Constants.Mouse.Delta);

            // Console.WriteLine("{0}, {1}", BoundingBox.Corner, BoundingBox.CornerDimensionOffset);
            if (CheckForMouseHover())
            {
                
                Console.WriteLine("hit");
                if (Globals.Mouse.IsButtonDown(MouseButton.Left))
                {

                    // MouseDeltaPositionOffset += Constants.Mouse.Delta * 2;
                    // Console.WriteLine("pressed.");
                    // Position += Constants.Mouse.Delta;
                    // SetAbsolutePositionOffset((5*Constants.Mouse.Delta.X, 5*-Constants.Mouse.Delta.Y));
                    //SetAbsolutePosition((Constants.Mouse.Position.X, Constants.HEIGHT - Constants.Mouse.Position.Y));
                    //Vector2 FlippedMousePosition = (Constants.Mouse.Position.X, Constants.HEIGHT - Constants.Mouse.Position.Y);
                    //BoundingBox.Position = FlippedMousePosition;
                    //SetAbsolutePosition(BoundingBox.Corner-CoordinateOffset);
                    //BoundingBox.Update();
                    // Console.WriteLine(Position);
                    // Update();
                    

                }
                

            }

        }

        public new void Update()
        {

            base.Update();

            BoundingBox.Update(this);

        }

        private bool CheckForMouseHover()
        {

            Vector2 MousePosition = Globals.Mouse.Position;

            Vector2 FlippedMousePosition = (MousePosition.X, Globals.HEIGHT - MousePosition.Y);
            // Console.WriteLine(FlippedMousePosition);
            if (FlippedMousePosition.X >= BoundingBox.Corner.X && FlippedMousePosition.X <= BoundingBox.CornerDimensionOffset.X
                && FlippedMousePosition.Y >= BoundingBox.Corner.Y && FlippedMousePosition.Y <= BoundingBox.CornerDimensionOffset.Y) 
            {

                return true;

            } else
            {

                return false;

            }
            
        }
    }
}
