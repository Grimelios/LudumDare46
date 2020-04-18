using GlmSharp;

namespace Engine.Shapes._2D
{
	public class Point2D : Shape2D
	{
		public Point2D() : this(vec2.Zero)
		{
		}

		public Point2D(vec2 position) : base(ShapeTypes2D.Point)
		{
			Position = position;
		}

		protected override bool Contains(vec2 p)
		{
			return p == vec2.Zero;
		}
	}
}
