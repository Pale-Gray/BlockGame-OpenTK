using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Blockgame_OpenTK.Font;
using Blockgame_OpenTK.Gui;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace Blockgame_OpenTK.Core.Gui 
{

    struct GuiVertex {

        public Vector2 Position;
        public Color4<Rgba> Color;

    }
    struct GuiElement {

        public Vector2 Position;
        public Vector2 Dimensions;
        public Vector2 Origin;
        public Color4<Rgba> Color;

    }

    struct GuiTextboxState
    {

        public bool IsFocused;
        public string Text;

    }

    struct GuiStates
    {

        public Dictionary<int, GuiTextboxState> TextboxStates = new();

        public GuiStates() {} 

    }

    public class GuiRenderer 
    {

        public static readonly Vector2 Center = (0.5f, 0.5f);
        private static readonly Color4<Rgba> _default = Color4.White;
        private static Shader _guiShader;
        private static Camera _guiCamera = new Camera((0, 0, 1), (0, 0, -1), (0, 1, 0), CameraType.Orthographic, 90);
        private static Dictionary<string, GuiStates> _guiStates = new();
        private static int _currentIndex = 0;
        private static string _currentKey = "";

        public static void Initialize() 
        {

            _guiShader = new Shader("immediategui.vert", "immediategui.frag");

        }

        public static void UpdateProjectionMatrix() {

            _guiCamera.UpdateProjectionMatrix();

        }

        public static void Begin(string guiName) 
        {

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(0, 0, (int)GlobalValues.Width, (int)GlobalValues.Height);
            _currentIndex = 0;
            _currentKey = guiName;
            if (!_guiStates.ContainsKey(guiName))
            {
                _guiStates.Add(guiName, new GuiStates());
            }

        }

        public static bool RenderButton(Vector2 position, Vector2 dimensions, Vector2 origin, string text, Color4<Rgba> color = default)
        {

            bool isClicked = false;
            if (color == default) color = _default;

            GuiElement element = new GuiElement
            {
                Position = position,
                Dimensions = dimensions,
                Origin = origin,
                Color = color
            };

            if (IsPointCollidingWithElement(Input.MousePosition, element))
            {
                element.Color = new Color4<Rgba>(color.X * 0.8f, color.Y * 0.8f, color.Z * 0.8f, color.W);
                if (Input.IsMouseButtonPressed(OpenTK.Platform.MouseButton.Button1))
                {
                    element.Color = new Color4<Rgba>(color.X * 0.5f, color.Y * 0.5f, color.Z * 0.5f, color.W);
                    isClicked = true;
                }
            }

            DrawElement(element);
            // CachedFontRenderer.RenderFont(out _, position, origin, 1000, 18, text);
            FontRenderer.Text(position - (dimensions * origin) + (dimensions * origin), dimensions, 24, Color4.Black, text);
            _currentIndex++;

            return isClicked;

        }
        static int _idx = 0;
        public static bool RenderTextbox(Vector2 position, Vector2 dimensions, Vector2 origin, out string text, Color4<Rgba> color = default)
        {

            bool wasEntered = false;

            if (color == default) color = Color4.Black; 
            if (!_guiStates[_currentKey].TextboxStates.ContainsKey(_currentIndex))
            {
                _guiStates[_currentKey].TextboxStates.Add(_currentIndex, new GuiTextboxState() { Text = string.Empty });
            }

            GuiTextboxState state = _guiStates[_currentKey].TextboxStates[_currentIndex];

            GuiElement element = new GuiElement()
            {
                Position = position,
                Dimensions = dimensions,
                Origin = origin,
                Color = color
            };

            if (IsPointCollidingWithElement(Input.MousePosition, element))
            {

                if (Input.IsMouseButtonPressed(MouseButton.Button1))
                {

                    state.IsFocused = true;

                }

            } else
            {

                if (Input.IsMouseButtonPressed(MouseButton.Button1))
                {

                    state.IsFocused = false;

                }

            }

            text = state.Text;

            if (state.IsFocused)
            {

                foreach (char character in Input.CurrentTypedChars)
                {

                    switch (character)
                    {
                        case '\e':
                        case '\r':
                        case '\n':
                            continue;
                        case '\b':
                            if (Input.IsKeyDown(Key.LeftControl) || Input.IsKeyDown(Key.RightControl))
                            {
                                int index = state.Text.Length - 1;
                                int length = 1;
                                for (int i = state.Text.Length - 1; i > 0; i--)
                                {

                                    if (state.Text[i-1] != ' ')
                                    {

                                        index--;
                                        length++;

                                    } else
                                    {

                                        if (state.Text[i] == ' ')
                                        {
                                            index--;
                                            length++;
                                            continue;
                                        }
                                        break;

                                    }

                                }
                                if (length > 1)
                                {
                                    state.Text = state.Text.Remove(index, length);
                                }
                            } else
                            {
                                if (state.Text.Length != 0)
                                {
                                    state.Text = state.Text.Substring(0, state.Text.Length - 1);
                                }
                            }   
                            continue;
                        default:
                            break;
                    }

                    state.Text += character;

                }

                if (Input.IsKeyPressed(Key.KeypadEnter) || Input.IsKeyPressed(Key.Return))
                {

                    wasEntered = true;
                    state.Text = string.Empty;

                }

            }

            DrawElement(element);
            FontRenderer.Text(position - (dimensions * origin) + (0, dimensions.Y), dimensions, 18, Color4.Black, _guiStates[_currentKey].TextboxStates[_currentIndex].Text, justifyMode: JustifyMode.HorizontalLeftVerticalCentered);

            _guiStates[_currentKey].TextboxStates[_currentIndex] = state;
            _currentIndex++;

            return wasEntered;

        }

        public static void RenderElement(Vector2 position, Vector2 dimensions, Vector2 origin, Color4<Rgba> color = default) {

            if (color == default) color = _default;

            GuiElement element = new GuiElement() 
            {
                Position = position,
                Dimensions = dimensions,
                Origin = origin,
                Color = color
            };

            DrawElement(element);
            _currentIndex++;

        }

        private static List<GuiVertex> _elementVertices = new();
        private static int[] _elementIndices = { 0, 1, 2, 2, 3, 0 };
        private static int _vbo, _ibo, _vao;
        private static void DrawElement(GuiElement element)
        {

            Vector2 lowerLeftBound;
            lowerLeftBound.X = element.Position.X - (element.Dimensions.X * element.Origin.X);
            lowerLeftBound.Y = element.Position.Y - (element.Dimensions.Y * element.Origin.Y) + element.Dimensions.Y;

            lowerLeftBound.Y = GlobalValues.Height - lowerLeftBound.Y;

            GL.Scissor((int)lowerLeftBound.X, (int)lowerLeftBound.Y, (int)element.Dimensions.X, (int)element.Dimensions.Y);
            
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ibo);
            GL.DeleteVertexArray(_vao);
            _elementVertices.Clear();

            _vbo = GL.GenBuffer();
            _ibo = GL.GenBuffer();
            _vao = GL.GenVertexArray();

            _elementVertices.AddRange(
                new GuiVertex() { Position = element.Position - (element.Dimensions * element.Origin), Color = element.Color },
                new GuiVertex() { Position = element.Position - (element.Dimensions * element.Origin) + (0, element.Dimensions.Y), Color = element.Color },
                new GuiVertex() { Position = element.Position - (element.Dimensions * element.Origin) + element.Dimensions, Color = element.Color },
                new GuiVertex() { Position = element.Position - (element.Dimensions * element.Origin) + (element.Dimensions.X, 0), Color = element.Color }   
            );

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData<GuiVertex>(BufferTarget.ArrayBuffer, _elementVertices.Count * Marshal.SizeOf<GuiVertex>(), CollectionsMarshal.AsSpan(_elementVertices), BufferUsage.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _elementIndices.Length * sizeof(int), _elementIndices, BufferUsage.DynamicDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<GuiVertex>(), Marshal.OffsetOf<GuiVertex>(nameof(GuiVertex.Position)));
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<GuiVertex>(), Marshal.OffsetOf<GuiVertex>(nameof(GuiVertex.Color)));
            GL.EnableVertexAttribArray(1);

            _guiShader.Use();
            GL.UniformMatrix4f(GL.GetUniformLocation(_guiShader.id, "view"), 1, true, ref _guiCamera.ViewMatrix);
            GL.UniformMatrix4f(GL.GetUniformLocation(_guiShader.id, "projection"), 1, true, ref _guiCamera.ProjectionMatrix);

            GL.DrawElements(PrimitiveType.Triangles, _elementIndices.Length, DrawElementsType.UnsignedInt, 0);

        }

        public static void End() 
        {

            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.ScissorTest);

        }

        private static bool IsPointCollidingWithElement(Vector2 point, GuiElement element)
        {

            if (point.X >= element.Position.X - (element.Origin.X * element.Dimensions.X) && point.X <= element.Position.X - (element.Origin.X * element.Dimensions.X) + element.Dimensions.X &&
                point.Y >= element.Position.Y - (element.Origin.Y * element.Dimensions.Y) && point.Y <= element.Position.Y - (element.Origin.Y * element.Dimensions.Y) + element.Dimensions.Y) return true;

            return false;

        }

    }

}