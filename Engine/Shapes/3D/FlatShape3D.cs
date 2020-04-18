using System.Diagnostics;
using Engine.Shapes._2D;
using GlmSharp;

namespace Engine.Shapes._3D
{
	// This represents any flat (i.e. 2D) shape with a 3D transform. Useful for combat calculations (plus probably
	// other uses).
	public class FlatShape3D : Shape3D
	{
		private Shape2D flatShape;

		public FlatShape3D() : this(null)
		{
		}

		public FlatShape3D(Shape2D flatShape, bool isOrientable = true) : base(ShapeTypes3D.Flat, isOrientable)
		{
			if (flatShape != null)
			{
				FlatShape = flatShape;
			}
		}

		public Shape2D FlatShape
		{
			get => flatShape;
			set
			{
				Debug.Assert(value != null, "Can't nullify a flat shape.");
				Debug.Assert(value.Type != ShapeTypes2D.Line, "Can't create a 3D version of a 2D line (just use " +
					"Line3D instead).");

				flatShape = value;
			}
		}

		protected override bool Contains(vec3 p)
		{
			// TODO: Finish this implementation (if needed).
			return false;
		}
	}
}
