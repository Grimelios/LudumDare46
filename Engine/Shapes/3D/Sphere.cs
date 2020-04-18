using Engine.Utility;
using GlmSharp;

namespace Engine.Shapes._3D
{
	public class Sphere : Shape3D
	{
		public static bool Contains(vec3 p, vec3 center, float radius)
		{
			return Utilities.DistanceSquared(p, center) <= radius * radius;
		}

		public Sphere() : this(0)
		{
		}

		// Spheres are marked orientable primarily for debug rendering. Orientation doesn't actually matter for overlap
		// calculations.
		public Sphere(float radius) : base(ShapeTypes3D.Sphere, true)
		{
			Radius = radius;
		}

		public float Radius { get; set; }

		protected override bool Contains(vec3 p)
		{
			return Utilities.LengthSquared(p) <= Radius * Radius;
		}
	}
}
