using System;
using Engine.Utility;
using GlmSharp;

namespace Engine.Shapes._2D
{
	public class Line2D : Shape2D
	{
		public static float Distance(vec2 p, vec2 e0, vec2 e1, out float t)
		{
			return (float)Math.Sqrt(DistanceSquared(p, e0, e1, out t));
		}

		public static float DistanceSquared(vec2 p, vec2 e0, vec2 e1, out float t)
		{
			var squared = Utilities.DistanceSquared(e0, e1);

			if (squared < Constants.DefaultEpsilon)
			{
				t = 0;

				return Utilities.DistanceSquared(e0, p);
			}

			// See https://stackoverflow.com/a/1501725/7281613.
			t = Math.Max(0, Math.Min(1, vec2.Dot(p - e0, e1 - e0) / squared));

			var projection = e0 + t * (e1 - e0);

			return Utilities.DistanceSquared(p, projection);
		}

		public Line2D() : this(vec2.Zero, vec2.Zero)
		{
		}

		public Line2D(vec2 p1, vec2 p2) : base(ShapeTypes2D.Line)
		{
			P1 = p1;
			P2 = p2;
		}

		public vec2 P1
		{
			get => Position;
			set => Position = value;
		}

		public vec2 P2 { get; set; }

		public vec2 Evaluate(float t)
		{
			return P1 + (P2 - P1) * t;
		}

		protected override bool Contains(vec2 p)
		{
			// TODO: Finish this implementation (if needed).
			return false;
		}

		public float Distance(vec2 p, out float t)
		{
			return Distance(p, P1, P2, out t);
		}

		public float DistanceSquared(vec2 p, out float t)
		{
			return DistanceSquared(p, P1, P2, out t);
		}
	}
}
