using System.Diagnostics;

namespace Engine.Core._2D
{
	[DebuggerDisplay("{X}, {Y}, {Width}, {Height}")]
	public struct SourceRect
	{
		public SourceRect(int x, int y, int width, int height)
		{
			Debug.Assert(width > 0 && height > 0, "Source rect dimensions must be positive.");

			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public int X { get; }
		public int Y { get; }
		public int Width { get; }
		public int Height { get; }

		public int Left => X;
		public int Right => X + Width - 1;
		public int Top => Y;
		public int Bottom => Y + Height - 1;
	}
}
