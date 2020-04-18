using System;
using System.Diagnostics;
using Engine.Utility;
using GlmSharp;

namespace Engine.Shapes._3D
{
	public class Capsule3D : Shape3D
	{
		private float radius;
		private float height;

		public Capsule3D() : this(0, 0)
		{
		}

		public Capsule3D(float radius, float height, bool isOrientable = true) : base(ShapeTypes3D.Capsule, isOrientable)
		{
			Radius = radius;
			Height = height;
		}

		public float Radius
		{
			get => radius;
			set
			{
				Debug.Assert(value >= 0, "PhysicsCapsule3D radius can't be negative.");

				radius = value;
			}
		}

		public float Height
		{
			get => height;
			set
			{
				Debug.Assert(value >= 0, "PhysicsCapsule3D height can't be negative.");

				height = value;
			}
		}

		protected override bool Contains(vec3 p)
		{
			if (IsOrientable)
			{
				p = InverseOrientation * p;
			}

			// Check hemispheres.
			var halfVector = new vec3(0, Height / 2, 0);
			var d1 = Utilities.DistanceSquared(p, halfVector);
			var d2 = Utilities.DistanceSquared(p, -halfVector);
			var rSquared = Radius * Radius;

			if (d1 <= rSquared || d2 <= rSquared)
			{
				return true;
			}

			// Check the cylinder.
			if (Math.Abs(p.y) > Height / 2)
			{
				return false;
			}

			return Utilities.LengthSquared(p.swizzle.xz) <= rSquared;
		}
	}
}
