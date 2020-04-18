using Engine.Core;
using Engine.Interfaces._2D;
using GlmSharp;

namespace Engine.Shapes._2D
{
	public abstract class Shape2D : Shape<Shape2D, ShapeTypes2D, vec2>, IPositionable2D, IRotatable
	{
		protected Shape2D(ShapeTypes2D type) : base(type)
		{
		}

		public float X
		{
			get => position.x;
			set => position.x = value;
		}

		public float Y
		{
			get => position.y;
			set => position.y = value;
		}

		public float Rotation { get; set; }

		public override bool Contains(vec2 p, Coordinates coords)
		{
			if (coords == Coordinates.Absolute)
			{
				p -= position;
			}

			return Contains(p);
		}

		public override bool Overlaps(Shape2D other)
		{
			return ShapeHelper2D.CheckOverlap(this, other);
		}
	}
}
