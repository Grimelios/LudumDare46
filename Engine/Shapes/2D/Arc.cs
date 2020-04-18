using Engine.Utility;
using GlmSharp;

namespace Engine.Shapes._2D
{
	public class Arc : Shape2D
	{
		public Arc() : this(0, 0, 0)
		{
		}

		public Arc(float radius, float spread) : this(radius, spread, 0)
		{
		}

		public Arc(float radius, float spread, float angle) : base(ShapeTypes2D.Arc)
		{
			Radius = radius;
			Spread = spread;
			Rotation = angle;
		}

		public float Radius { get; set; }
		public float Spread { get; set; }
		public float Angle
		{
			get => Rotation;
			set => Rotation = value;
		}

		protected override bool Contains(vec2 p)
		{
			if (Utilities.LengthSquared(p) > Radius * Radius)
			{
				return false;
			}

			var angle = Utilities.Angle(p);
			var delta = Utilities.Delta(angle, Angle);

			return delta <= Spread / 2;
		}
	}
}
