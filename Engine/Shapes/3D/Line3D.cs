using System;
using Engine.Utility;
using GlmSharp;

namespace Engine.Shapes._3D
{
	// Note that for lines, orientation is always left as the identity quaternion.
	public class Line3D : Shape3D
	{
		public static float Distance(vec3 p, vec3 e0, vec3 e1)
		{
			return (float)Math.Sqrt(DistanceSquared(p, e0, e1));
		}

		public static float Distance(vec3 p, vec3 e0, vec3 e1, out float t)
		{
			return (float)Math.Sqrt(DistanceSquared(p, e0, e1, out t));
		}

		// This version of the function removes a length calculation that's (sometimes) necessary to compute T.
		public static float DistanceSquared(vec3 p, vec3 e0, vec3 e1)
		{
			var v1 = p - e0;
			var v2 = e1 - e0;

			if (Utilities.Dot(v1, v2) <= 0)
			{
				return Utilities.DistanceSquared(p, e0);
			}

			var v3 = p - e1;

			if (Utilities.Dot(v2, v3) >= 0)
			{
				return Utilities.DistanceSquared(p, e1);
			}

			var projected = e0 + Utilities.Project(v1, v2);

			return Utilities.DistanceSquared(p, projected);
		}

		public static float DistanceSquared(vec3 p, vec3 e0, vec3 e1, out float t)
		{
			var v1 = p - e0;
			var v2 = e1 - e0;

			if (Utilities.Dot(v1, v2) <= 0)
			{
				t = 0;

				return Utilities.DistanceSquared(p, e0);
			}

			var v3 = p - e1;

			if (Utilities.Dot(v2, v3) >= 0)
			{
				t = 1;

				return Utilities.DistanceSquared(p, e1);
			}

			var projected = e0 + Utilities.Project(v1, v2);
			var d1 = Utilities.Distance(projected, e0);
			var d2 = Utilities.Length(v2);

			t = d1 / d2;

			return Utilities.DistanceSquared(p, projected);
		}

		// This is very similar to the function above, but different enough to warrant the repetition.
		public static vec3 ClosestPoint(vec3 p, vec3 e0, vec3 e1)
		{
			var v1 = p - e0;
			var v2 = e1 - e0;

			if (Utilities.Dot(v1, v2) <= 0)
			{
				return e0;
			}

			var v3 = p - e1;

			if (Utilities.Dot(v1, v3) >= 0)
			{
				return e1;
			}

			var projected = e0 + Utilities.Project(v1, v2);
			var d1 = Utilities.Distance(projected, e0);
			var d2 = Utilities.Length(v2);
			var t = d1 / d2;

			return e0 + (e1 - e0) * t;
		}

		public Line3D() : this(vec3.Zero, vec3.Zero)
		{
		}

		public Line3D(vec3 p1, vec3 p2) : base(ShapeTypes3D.Line, false)
		{
			P1 = p1;
			P2 = p2;
			IsOrientable = false;
		}

		public vec3 P1
		{
			get => Position;
			set => Position = value;
		}

		public vec3 P2 { get; set; }

		public vec3 Evaluate(float t)
		{
			return P1 + (P2 - P1) * t;
		}

		protected override bool Contains(vec3 p)
		{
			// TODO: Finish this implementation (if needed).
			return false;
		}

		public float Distance(vec3 p, out float t)
		{
			return Distance(p, P1, P2, out t);
		}

		public float DistanceSquared(vec3 p, out float t)
		{
			return DistanceSquared(p, P1, P2, out t);
		}
	}
}
