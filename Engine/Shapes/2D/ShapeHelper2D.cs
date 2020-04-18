using System.Linq;
using Engine.Core;
using Engine.Utility;
using GlmSharp;

namespace Engine.Shapes._2D
{
	public static class ShapeHelper2D
	{
		public static bool CheckOverlap(Shape2D shape1, Shape2D shape2)
		{
			var type1 = shape1.Type;
			var type2 = shape2.Type;

			if (type1 == ShapeTypes2D.Point)
			{
				return shape2.Contains(shape1.Position, Coordinates.Absolute);
			}

			if (type2 == ShapeTypes2D.Point)
			{
				return shape1.Contains(shape2.Position, Coordinates.Absolute);
			}

			// By ensuring that the second shape sorts after the first (as far as shape type), the total number of
			// switch cases can be lowered.
			if ((int)type1 > (int)type2)
			{
				var temp = shape1;
				shape1 = shape2;
				shape2 = temp;
				type1 = type2;
			}

			switch (type1)
			{
				case ShapeTypes2D.Arc: return CheckOverlap((Arc)shape1, shape2);
				case ShapeTypes2D.Circle: return CheckOverlap((Circle)shape1, shape2);
				case ShapeTypes2D.Rectangle: return CheckOverlap((Rectangle)shape1, (Rectangle)shape2);
			}

			return false;
		}

		private static bool CheckOverlap(Arc arc, Shape2D other)
		{
			switch (other.Type)
			{
				case ShapeTypes2D.Arc: return CheckOverlap(arc, (Arc)other);
				case ShapeTypes2D.Circle: return CheckOverlap(arc, (Circle)other);
				case ShapeTypes2D.Rectangle: return CheckOverlap(arc, (Rectangle)other);
			}

			return false;
		}

		private static bool CheckOverlap(Circle circle, Shape2D other)
		{
			switch (other.Type)
			{
				case ShapeTypes2D.Circle: return CheckOverlap(circle, (Circle)other);
				case ShapeTypes2D.Rectangle: return CheckOverlap(circle, (Rectangle)other);
			}

			return false;
		}

		private static bool CheckOverlap(Arc arc1, Arc arc2)
		{
			// TODO: Finish this implementation.
			return false;
		}

		private static bool CheckOverlap(Arc arc, Circle circle)
		{
			var p1 = arc.Position;
			var p2 = circle.Position;
			var sumRadius = arc.Radius + circle.Radius;
			var distanceSquared = Utilities.DistanceSquared(p1, p2);

			if (distanceSquared > sumRadius * sumRadius)
			{
				return false;
			}

			var angle = Utilities.Angle(p1, p2);
			var delta = Utilities.Delta(angle, arc.Angle);
			var halfSpread = arc.Spread / 2;

			if (delta <= halfSpread)
			{
				return true;
			}

			var points = new vec2[2];

			for (int i = 0; i < 2; i++)
			{
				var a = arc.Angle + halfSpread * (i == 0 ? -1 : 1);

				points[i] = p1 + Utilities.Direction(a) * arc.Radius;
			}

			if (points.Any(p => circle.Contains(p, Coordinates.Absolute)))
			{
				return true;
			}

			var squared = circle.Radius * circle.Radius;

			return points.Any(p => Utilities.DistanceSquaredToLine(p2, p1, p, out _) <= squared);
		}

		private static bool CheckOverlap(Arc arc, Rectangle rect)
		{
			// TODO: Finish this implementation.
			return false;
		}

		private static bool CheckOverlap(Circle circle1, Circle circle2)
		{
			var sum = circle1.Radius + circle2.Radius;
			var squared = Utilities.DistanceSquared(circle1.Position, circle2.Position);

			return squared <= sum * sum;
		}

		private static bool CheckOverlap(Circle circle, Rectangle rect)
		{
			// TODO: Finish this implementation.
			return false;
		}

		private static bool CheckOverlap(Rectangle rect1, Rectangle rect2)
		{
			// TODO: Finish this implementation.
			return false;
		}
	}
}
