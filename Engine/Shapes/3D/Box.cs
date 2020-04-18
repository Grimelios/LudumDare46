using System;
using System.Diagnostics;
using GlmSharp;

namespace Engine.Shapes._3D
{
	public class Box : Shape3D
	{
		private float width;
		private float height;
		private float depth;

		public Box() : this(0, 0, 0)
		{
		}

		// This constructor creates a cube (i.e. all dimensions area the same size).
		public Box(float size, bool isOrientable = true) : this(size, size, size, isOrientable)
		{
		}

		public Box(float width, float height, float depth, bool isOrientable = true) :
			base(ShapeTypes3D.Box, isOrientable)
		{
			Width = width;
			Height = height;
			Depth = depth;
		}

		public float Width
		{
			get => width;
			set
			{
				Debug.Assert(value >= 0, "PhysicsBox width can't be negative.");

				width = value;
			}
		}

		public float Height
		{
			get => height;
			set
			{
				Debug.Assert(value >= 0, "PhysicsBox height can't be negative.");

				height = value;
			}
		}

		public float Depth
		{
			get => depth;
			set
			{
				Debug.Assert(value >= 0, "PhysicsBox depth can't be negative.");

				depth = value;
			}
		}

		public vec3 Bounds
		{
			get => new vec3(width, height, depth);
			set
			{
				Width = value.x;
				Height = value.y;
				Depth = value.z;
			}
		}

		protected override bool Contains(vec3 p)
		{
			if (IsOrientable)
			{
				p = InverseOrientation * p;
			}

			return Math.Abs(p.x) <= width / 2 && Math.Abs(p.y) <= height / 2 && Math.Abs(p.z) <= depth / 2;
		}
	}
}
