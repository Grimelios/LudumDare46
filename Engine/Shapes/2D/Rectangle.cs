using System;
using System.Diagnostics;
using Engine.Utility;
using GlmSharp;

namespace Engine.Shapes._2D
{
	public class Rectangle : Shape2D
	{
		private float width;
		private float height;

		public Rectangle() : this(0, 0, 0, 0)
		{
		}

		public Rectangle(float size) : this(0, 0, size, size)
		{
		}

		public Rectangle(float width, float height) : this(0, 0, width, height)
		{
		}

		public Rectangle(float x, float y, float size) : this(x, y, size, size)
		{
		}

		public Rectangle(float x, float y, float width, float height) : base(ShapeTypes2D.Rectangle)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public float Width
		{
			get => width;
			set
			{
				Debug.Assert(value >= 0, "Rectangle width must be positive.");

				width = value;
			}
		}

		public float Height
		{
			get => height;
			set
			{
				Debug.Assert(value >= 0, "Rectangle height must be positive.");

				height = value;
			}
		}

		public float Left
		{
			get => position.x - width / 2;
			set => position.x = value + width / 2;
		}

		public float Right
		{
			get => position.x + width / 2;
			set => position.x = value - width / 2;
		}

		public float Top
		{
			get => position.y - height / 2;
			set => position.y = value + height / 2;
		}

		public float Bottom
		{
			get => position.y + height / 2;
			set => position.y = value - height / 2;
		}

		public vec2 Dimensions => new vec2(width, height);

		public vec2[] Corners
		{
			get
			{
				var halfWidth = width / 2;
				var halfHeight = height / 2;

				var points = new []
				{
					new vec2(-halfWidth, -halfHeight),
					new vec2(halfWidth, -halfHeight),
					new vec2(halfWidth, halfHeight),
					new vec2(-halfWidth, halfHeight)
				};

				if (Rotation != 0)
				{
					var matrix = Utilities.RotationMatrix2D(Rotation);

					for (int i = 0; i < points.Length; i++)
					{
						points[i] = matrix * points[i];
					}
				}

				for (int i = 0; i < points.Length; i++)
				{
					points[i] += position;
				}

				return points;
			}
		}

		public Rectangle Resize(float amount)
		{
			Debug.Assert(amount >= -width / 2 && amount >= -height / 2, "Cannot shrink a rectangle to negative " +
				"dimensions.");

			return new Rectangle(position.x - amount, position.y - amount, width + amount * 2, height + amount * 2);
		}

		protected override bool Contains(vec2 p)
		{
			if (Rotation != 0)
			{
				p = Utilities.Rotate(p, -Rotation);
			}

			return Math.Abs(p.x) <= width / 2 && Math.Abs(p.y) <= height / 2;
		}
	}
}
