﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Duality;
using Duality.Drawing;
using Duality.Components;
using Duality.Components.Physics;

namespace Duality.Editor.Plugins.CamView.CamViewStates
{
	// RigidBodyEditorSelVertices Test 2
	public class PolygonRigidBodyEditorOverlay
	{
		public enum VertexType
		{
			None = 0,
			PosibleSelect = 1, // Can be selected
			PosibleNew = 2, // Can be added
			Selected = 3 // Is selected
		}

		public class VertexInfo
		{
			public PolyShapeInfo shape;
			public VertexType type;
			public int id = -1;
			public Vector2 pos;
		}

		private const float SELECTOR_RADIUS = 5f;

		private VertexInfo currentVertex = new VertexInfo();
		
		public VertexInfo CurrentVertex
		{
			get { return this.currentVertex; } 
			set { this.currentVertex = value; }
		}

		public void Draw(RigidBody body, Canvas canvas, Vector3 mousePos)
		{
			if (body == null) return;

			float radius = SELECTOR_RADIUS / MathF.Max(0.0001f, canvas.DrawDevice.GetScaleAtZ(0f));

			canvas.State.ColorTint = ColorRgba.White;
			if (currentVertex.type == VertexType.Selected)
			{
				canvas.DrawCircle(currentVertex.pos.X, currentVertex.pos.Y, radius * 2f); // Draw selected vertex circle (single mode is different that multiple/pinned mode)
			}
			else
			{
				currentVertex = new VertexInfo();
			}

			// Prepare the transform matrix for this object, so 
			// we can move the RigidBody vertices into world space quickly
			Transform transform = body.GameObj.Transform;
			Vector2 bodyPos = transform.Pos.Xy;
			Vector2 bodyDotX;
			Vector2 bodyDotY;
			MathF.GetTransformDotVec(transform.Angle, transform.Scale, out bodyDotX, out bodyDotY);

			IEnumerable<PolyShapeInfo> shapes = body.Shapes.Where(x => x.GetType() == typeof(PolyShapeInfo)).Cast<PolyShapeInfo>();
			foreach (PolyShapeInfo shape in shapes)
			{
				Vector2[] vertices = shape.Vertices;
				for (int i = 0; i < vertices.Length; i++)
				{
					int iNext = i < vertices.Length - 1 ? i + 1 : 0; // This works only if the shape is a closed polygon
					Vector2 pA = vertices[i];
					Vector2 pB = vertices[iNext];

					// Transform vertices from object space to world space
					MathF.TransformDotVec(ref pA, ref bodyDotX, ref bodyDotY);
					MathF.TransformDotVec(ref pB, ref bodyDotX, ref bodyDotY);
					pA += bodyPos;
					pB += bodyPos;

					canvas.FillCircle(pA.X, pA.Y, radius); // Draw vertex

					if (currentVertex.type == VertexType.None) // Try to find a posible action (select or new)
					{
						if (MathF.Distance(pA.X, pA.Y, mousePos.X, mousePos.Y) <= radius) // Posible selection point found
						{
							currentVertex = new VertexInfo() { shape = shape, type = VertexType.PosibleSelect, id = i, pos = pA };
							canvas.DrawCircle(pA.X, pA.Y, radius * 2f);
						}
						else if (MathF.Distance(pB.X, pB.Y, mousePos.X, mousePos.Y) > radius) // Posible new point found
						{
							Vector2 p = MathF.PointLineNearestPoint(mousePos.X, mousePos.Y, pA.X, pA.Y, pB.X, pB.Y);
							if (MathF.Distance(p.X, p.Y, mousePos.X, mousePos.Y) < radius)
							{
								currentVertex = new VertexInfo() { shape = shape, type = VertexType.PosibleNew, id = i, pos = p };

								canvas.DrawCircle(p.X, p.Y, radius);
								canvas.DrawCircle(p.X, p.Y, radius * 2f);
							}
						}
					}
				}
			}
		}
	}
}