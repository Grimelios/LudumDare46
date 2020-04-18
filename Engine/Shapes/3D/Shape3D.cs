using Engine.Core;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Shapes._3D
{
	public abstract class Shape3D : Shape<Shape3D, ShapeTypes3D, vec3>, ITransformable3D
	{
		private quat orientation;

		protected Shape3D(ShapeTypes3D type, bool isOrientable) : base(type)
		{
			orientation = quat.Identity;
			InverseOrientation = quat.Identity;
			IsOrientable = isOrientable;
		}

		public quat Orientation
		{
			get => orientation;
			set
			{
				// Ideally, this property would never be called for non-orientable physicsShapes3D. In practice, though, that's
				// difficult to enforce (and likely pretty inefficient anyway), so it's much simpler to just return.
				if (!IsOrientable)
				{
					return;
				}

				orientation = value;
				InverseOrientation = value.Inverse;
			}
		}

		public quat InverseOrientation { get; private set; }

		// This allows overlap calculations to be optimized in many cases. Permanently false for lines (it's true for
		// spheres, but that doesn't have any affect on overlap calculations anyway).
		public bool IsOrientable { get; set; }

		public void SetTransform(vec3 p, quat q)
		{
			position = p;
			orientation = q;
		}

		public override bool Contains(vec3 p, Coordinates coords)
		{
			if (coords == Coordinates.Absolute)
			{
				p -= position;
			}

			return Contains(p);
		}

		public override bool Overlaps(Shape3D other)
		{
			return ShapeHelper3D.Overlaps(this, other);
		}
	}
}
