using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Blockgame_OpenTK.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

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

    public class GuiRenderer 
    {
        private static SortedDictionary<float, List<GuiElement>> _elements = new();
        private static List<(int vao, int vbo, int ibo)> _layerBuffers = new();
        private static readonly Color4<Rgba> _default = Color4.White;
        private static Shader _guiShader;
        private static Camera _guiCamera = new Camera((0, 0, 1), (0, 0, -1), (0, 1, 0), CameraType.Orthographic, 90);

        public static void Initialize() 
        {

            _guiShader = new Shader("immediategui.vert", "immediategui.frag");

        }

        public static void UpdateProjectionMatrix() {

            _guiCamera.UpdateProjectionMatrix();

        }

        public static void GuiBegin() 
        {

            for (int i = 0; i < _layerBuffers.Count; i++) 
            {
                GL.DeleteBuffer(_layerBuffers[i].vbo);
                GL.DeleteBuffer(_layerBuffers[i].ibo);
                GL.DeleteVertexArray(_layerBuffers[i].vao);
            }
            _layerBuffers.Clear();
            _elements.Clear();

        }

        public static void RenderElement(int layer, Vector2 position, Vector2 dimensions, Vector2 origin, Color4<Rgba> color = default) {

            if (color == default) color = _default;

            GuiElement element = new GuiElement() 
            {
                Position = position,
                Dimensions = dimensions,
                Origin = origin,
                Color = color
            };

            if (!_elements.ContainsKey(layer)) {
                _elements.Add(layer, [ element ]);
            } else {
                _elements[layer].Add(element);
            }

        }

        public static void GuiEnd() 
        {

            GL.Disable(EnableCap.DepthTest);
            foreach (KeyValuePair<float, List<GuiElement>> elementList in _elements) {

                List<GuiVertex> vertices = new();
                List<int> indices = new();
                foreach (GuiElement element in elementList.Value) {

                    vertices.AddRange(new GuiVertex() { Position = element.Position, Color = element.Color},
                                      new GuiVertex() { Position = element.Position + (0, element.Dimensions.Y), Color = element.Color},
                                      new GuiVertex() { Position = element.Position + element.Dimensions, Color = element.Color},
                                      new GuiVertex() { Position = element.Position + (element.Dimensions.X, 0), Color = element.Color});

                }

                for (int i = 0; i < vertices.Count / 4; i++) {

                    indices.AddRange(0 + (i*4),
                                     1 + (i*4),
                                     2 + (i*4),
                                     2 + (i*4),
                                     3 + (i*4),
                                     0 + (i*4));

                }

                int vao = GL.GenVertexArray();
                GL.BindVertexArray(vao);

                int vbo = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData<GuiVertex>(BufferTarget.ArrayBuffer, vertices.Count * Marshal.SizeOf<GuiVertex>(), CollectionsMarshal.AsSpan(vertices), BufferUsage.DynamicDraw);

                int ibo = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GL.BufferData<int>(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(int), CollectionsMarshal.AsSpan(indices), BufferUsage.DynamicDraw);

                GL.BindVertexArray(vao);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<GuiVertex>(), Marshal.OffsetOf<GuiVertex>(nameof(GuiVertex.Position)));
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<GuiVertex>(), Marshal.OffsetOf<GuiVertex>(nameof(GuiVertex.Color)));
                GL.EnableVertexAttribArray(1);

                _guiShader.Use();
                GL.UniformMatrix4f(GL.GetUniformLocation(_guiShader.id, "view"), 1, true, ref _guiCamera.ViewMatrix);
                GL.UniformMatrix4f(GL.GetUniformLocation(_guiShader.id, "projection"), 1, true, ref _guiCamera.ProjectionMatrix);

                GL.DrawElements(PrimitiveType.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);
                _layerBuffers.Add((vao, vbo, ibo));

            }
            GL.Enable(EnableCap.DepthTest);

        }

    }

}