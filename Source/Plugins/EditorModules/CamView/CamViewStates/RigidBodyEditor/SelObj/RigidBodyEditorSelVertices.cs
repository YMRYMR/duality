﻿using Duality.Components.Physics;
using Duality.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Duality.Editor.Plugins.CamView.CamViewStates
{
    // RigidBodyEditorSelVertices Test
    public class RigidBodyEditorSelVertices
    {
        public enum VertexType
        {
            None = 0,
            PosibleSelect = 1, // Can be selected
            PosibleNew = 2, // Can be added
            Selected = 3 // Is selected
        }

        public struct VertexInfo
        {
            public PolyShapeInfo shape;
            public VertexType type;
            public int id;
            public Vector2 pos;
        }

        private const float SELECTOR_RADIUS = 5f;

        private bool rendering = false;
        private PolyShapeInfo shape = null;
        private List<Vector2> selectedVertices = new List<Vector2>();
        private VertexInfo currentVertex = new VertexInfo();

        public PolyShapeInfo Shape { get { return shape; } set { shape = value; selectedVertices = new List<Vector2>(); } }
        // This property would be used by RigidBodyShapeCamViewLayer to know if it should add or edit a vertex on mouse left click
        public VertexInfo CurrentVertex { get { return currentVertex; } }

        public void Draw(Canvas canvas, Vector3 mousePos)
        {
            if (shape == null) return;

            if (!rendering)
            {
                rendering = true;

                mousePos = canvas.DrawDevice.GetSpaceCoord(mousePos);
                float radius = SELECTOR_RADIUS / MathF.Max(0.0001f, canvas.DrawDevice.GetScaleAtZ(0f));

                if (currentVertex.type == VertexType.Selected)
                {
                    canvas.FillCircleSegment(currentVertex.pos.X, currentVertex.pos.Y, 0f, radius * 3f, 0f, MathF.RadAngle360, radius * .5f);
                }
                else
                {
                    currentVertex = new VertexInfo();
                }

                canvas.PushState();
                canvas.State.ColorTint = ColorRgba.White;

                Vector2[] vertices = shape.Vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    int iNext = i < vertices.Length - 1 ? i + 1 : 0; // This works only if the shape is a closed polygon
                    Vector2 pA = vertices[i];
                    Vector2 pB = vertices[iNext];

                    canvas.FillCircle(pA.X, pA.Y, radius); // Draw vertex

                    if (currentVertex.type == VertexType.None) // Try to find a posible action (select or new)
                    {
                        if (MathF.Distance(pA.X, pA.Y, mousePos.X, mousePos.Y) <= radius) // Posible selection point found
                        {
                            currentVertex = new VertexInfo() { shape = this.shape, type = VertexType.PosibleSelect, id = i, pos = pA };
                            canvas.DrawCircle(pA.X, pA.Y, radius * 2f);
                        }
                        else if (MathF.Distance(pB.X, pB.Y, mousePos.X, mousePos.Y) > radius) // Posible new point found
                        {
                            Vector2 p = MathF.PointLineNearestPoint(mousePos.X, mousePos.Y, pA.X, pA.Y, pB.X, pB.Y);
                            if (MathF.Distance(p.X, p.Y, mousePos.X, mousePos.Y) < radius)
                            {
                                currentVertex = new VertexInfo() { shape = this.shape, type = VertexType.PosibleNew, id = i, pos = p };

                                canvas.PushState();
                                canvas.State.ColorTint = ColorRgba.White;
                                canvas.DrawCircle(p.X, p.Y, radius);
                                canvas.DrawCircle(p.X, p.Y, radius * 2f);
                                canvas.PopState();
                            }
                        }
                    }
                }

                canvas.PopState();

                rendering = false;
            }
        }
    }
}