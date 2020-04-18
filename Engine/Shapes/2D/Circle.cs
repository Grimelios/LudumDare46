using Engine.Utility;
using GlmSharp;

namespace Engine.Shapes._2D
{
	public class Circle : Shape2D
	{
		public Circle() : this(vec2.Zero, 0)
		{
		}

		public Circle(float radius) : this(vec2.Zero, radius)
		{
		}

		public Circle(vec2 position, float radius) : base(ShapeTypes2D.Circle)
		{
			Position = position;
			Radius = radius;
		}

		public float Radius { get; set; }

		protected override bool Contains(vec2 p)
		{
			return Utilities.LengthSquared(p) <= Radius * Radius;
		}
	}
}
