using System.Diagnostics;
using Engine.Shapes._2D;
using GlmSharp;

namespace Engine.Core._2D
{
	[DebuggerDisplay("{X}, {Y}, {Width}, {Height}")]
	public class Bounds2D
	{
		private int width;
		private int height;

		public Bounds2D() : this(0, 0, 0, 0)
		{
		}

		public Bounds2D(int size) : this(0, 0, size, size)
		{
		}

		public Bounds2D(int width, int height) : this(0, 0, width, height)
		{
		}

		public Bounds2D(int x, int y, int width, int height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public int X { get; set; }
		public int Y { get; set; }

		public int Width
		{
			get => width;
			set
			{
				Debug.Assert(value >= 0, "Bounds width must be non-negative.");

				width = value;
			}
		}

		public int Height
		{
			get => height;
			set
			{
				Debug.Assert(value >= 0, "Bounds height must be non-negative.");

				height = value;
			}
		}

		public int Left
		{
			get => X;
			set => X = value;
		}

		public int Right
		{
			get => X + Width - 1;
			set => X = value - Width + 1;
		}

		public int Top
		{
			get => Y;
			set => Y = value;
		}

		public int Bottom
		{
			get => Y + Height - 1;
			set => Y = value - Height + 1;
		}

		public ivec2 Location
		{
			get => new ivec2(X, Y);
			set
			{
				X = value.x;
				Y = value.y;
			}
		}

		public ivec2 Center
		{
			get => new ivec2(X + Width / 2, Y + Height / 2);
			set
			{
				X = value.x - Width / 2;
				Y = value.y - Height / 2;
			}
		}

		public ivec2 Dimensions
		{
			get => new ivec2(Width, Height);
			set
			{
				Width = value.x;
				Height = value.y;
			}
		}

		public ivec2[] Corners => new[]
		{
			new ivec2(X, Y),
			new ivec2(Right, Y),
			new ivec2(Right, Bottom),
			new ivec2(X, Bottom)
		};

		public bool Contains(ivec2 point)
		{
			return Contains(point.x, point.y);
		}

		public bool Contains(vec2 point)
		{
			return Contains(point.x, point.y);
		}

		private bool Contains(float x, float y)
		{
			return x >= Left && x <= Right && y >= Top && y <= Bottom;
		}

		public Bounds2D Resize(int amount)
		{
			Debug.Assert(amount >= -Width / 2 && amount >= -Height / 2, "Cannot shrink bounds to negative " +
				"dimensions.");

			return new Bounds2D(X - amount, Y - amount, Width + amount * 2, Height + amount * 2);
		}

		public Rectangle ToRectangle()
		{
			// Rectangles are positioned by their center.
			return new Rectangle(X + Width / 2, Y + Height / 2, Width, Height);
		}
	}
}
