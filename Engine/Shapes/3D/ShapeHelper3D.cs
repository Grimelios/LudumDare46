using System;
using System.Linq;
using Engine.Core;
using Engine.Utility;
using GlmSharp;

namespace Engine.Shapes._3D
{
	public static class ShapeHelper3D
	{
		public static bool Overlaps(Shape3D shape1, Shape3D shape2)
		{
			var type1 = shape1.Type;
			var type2 = shape2.Type;

			if (type1 == ShapeTypes3D.Point)
			{
				return shape2.Contains(shape1.Position, Coordinates.Absolute);
			}

			if (type2 == ShapeTypes3D.Point)
			{
				return shape1.Contains(shape2.Position, Coordinates.Absolute);
			}

			if ((int)type1 > (int)type2)
			{
				var temp1 = shape1;
				shape1 = shape2;
				shape2 = temp1;
				type1 = type2;
			}

			switch (type1)
			{
				case ShapeTypes3D.Box: return Overlaps((Box)shape1, shape2);
				case ShapeTypes3D.Capsule: return Overlaps((Capsule3D)shape1, shape2);
				case ShapeTypes3D.Cylinder: return Overlaps((Cylinder)shape1, shape2);
				case ShapeTypes3D.Flat: return Overlaps((FlatShape3D)shape1, shape2);
				case ShapeTypes3D.Line: return Overlaps((Line3D)shape1, shape2);
				case ShapeTypes3D.Sphere: return Overlaps((Sphere)shape1, (Sphere)shape2);
			}

			return false;
		}

		private static bool Overlaps(Box box, Shape3D other)
		{
			switch (other.Type)
			{
				case ShapeTypes3D.Box: return Overlaps(box, (Box)other);
				case ShapeTypes3D.Capsule: return Overlaps(box, (Capsule3D)other);
				case ShapeTypes3D.Cylinder: return Overlaps(box, (Cylinder)other);
				case ShapeTypes3D.Line: return Overlaps(box, (Line3D)other);
				case ShapeTypes3D.Sphere: return Overlaps(box, (Sphere)other);
			}

			return false;
		}

		private static bool Overlaps(Box box1, Box box2)
		{
			// TODO: Finish this implementation.
			return false;
		}

		private static bool Overlaps(Box box, Capsule3D capsule)
		{
			// TODO: Finish this implementation.
			return false;
		}

		private static bool Overlaps(Box box, Cylinder cylinder)
		{
			// TODO: Finish this implementation.
			return false;
		}

		private static bool Overlaps(Box box, Line3D line)
		{
			// TODO: Finish this implementation.
			return false;
		}

		private static bool Overlaps(Box box, Sphere sphere)
		{
			var p1 = box.Position;
			var p = sphere.Position - p1;

			if (box.IsOrientable)
			{
				p = box.InverseOrientation * p;
			}

			if (box.Contains(p, Coordinates.Relative))
			{
				return true;
			}

			var halfBounds = box.Bounds / 2;
			var dX = Math.Abs(p.x);
			var dY = Math.Abs(p.y);
			var dZ = Math.Abs(p.z);
			var r = sphere.Radius;

			bool withinX = Math.Abs(p.x) <= halfBounds.x;
			bool withinY = Math.Abs(p.y) <= halfBounds.y;
			bool withinZ = Math.Abs(p.z) <= halfBounds.z;

			// Two axes overlap.
			if (withinX && withinY && !withinZ)
			{
				return dZ <= halfBounds.z + r;
			}

			if (withinX && !withinY && withinZ)
			{
				return dY <= halfBounds.y + r;
			}

			if (!withinX && withinY && withinZ)
			{
				return dX <= halfBounds.x + r;
			}

			// No axes overlap (which means the relevant box corner needs to be checked).
			if (!(withinX || withinY || withinZ))
			{
				var x = halfBounds.x * (p.x < 0 ? -1 : 1);
				var y = halfBounds.y * (p.y < 0 ? -1 : 1);
				var z = halfBounds.z * (p.z < 0 ? -1 : 1);

				return Sphere.Contains(new vec3(x, y, z), p, r);
			}

			// Only a single axis overlaps.
			vec3 endpoint1;
			vec3 endpoint2;

			if (withinX)
			{
				var y = halfBounds.y * (p.y < 0 ? -1 : 1);
				var z = halfBounds.z * (p.z < 0 ? -1 : 1);

				endpoint1 = new vec3(halfBounds.x, y, z);
				endpoint2 = new vec3(-halfBounds.x, y, z);
			}
			else if (withinY)
			{
				var x = halfBounds.x * (p.x < 0 ? -1 : 1);
				var z = halfBounds.z * (p.z < 0 ? -1 : 1);

				endpoint1 = new vec3(x, halfBounds.y, z);
				endpoint2 = new vec3(x, -halfBounds.y, z);
			}
			else
			{
				var x = halfBounds.x * (p.x < 0 ? -1 : 1);
				var y = halfBounds.y * (p.y < 0 ? -1 : 1);

				endpoint1 = new vec3(x, y, halfBounds.z);
				endpoint2 = new vec3(x, y, -halfBounds.z);
			}

			return Utilities.DistanceSquaredToLine(p, endpoint1, endpoint2, out _) <= r * r;
		}

		private static bool Overlaps(Capsule3D capsule, Shape3D other)
		{
			switch (other.Type)
			{
				case ShapeTypes3D.Capsule: return Overlaps(capsule, (Capsule3D)other);
				case ShapeTypes3D.Cylinder: return Overlaps(capsule, (Cylinder)other);
				case ShapeTypes3D.Line: return Overlaps(capsule, (Line3D)other);
				case ShapeTypes3D.Sphere: return Overlaps(capsule, (Sphere)other);
			}

			return false;
		}

		private static bool Overlaps(Capsule3D capsule1, Capsule3D capsule2)
		{
			// TODO: Finish this implementation.
			return false;
		}

		private static bool Overlaps(Capsule3D capsule, Cylinder cylinder)
		{
			// TODO: Finish this implementation.
			return false;
		}

		private static bool Overlaps(Capsule3D capsule, Line3D line)
		{
			var center = capsule.Position;
			var halfHeight = capsule.Height / 2;
			var halfVector = new vec3(0, halfHeight, 0);
			var p1 = line.P1 - center;
			var p2 = line.P2 - center;
			var r = capsule.Radius;

			if (capsule.IsOrientable)
			{
				var inverse = capsule.InverseOrientation;
				p1 = inverse * p1;
				p2 = inverse * p2;
			}

			// One (or both) of the line endpoints are contained within the spherical caps.
			if (Sphere.Contains(p1, halfVector, r) || Sphere.Contains(p2, halfVector, r) ||
				Sphere.Contains(p1, -halfVector, r) || Sphere.Contains(p2, -halfVector, r))
			{
				return true;
			}
			
			var d1 = Utilities.LengthSquared(p1.swizzle.xz);
			var d2 = Utilities.LengthSquared(p2.swizzle.xz);
			var rSquared = r * r;

			// One (or both) of the line endpoints are contained within the cylinder.
			if ((d1 <= rSquared && Math.Abs(p1.y) <= halfHeight) ||
				(d2 <= rSquared && Math.Abs(p2.y) <= halfHeight))
			{
				return true;
			}

			d1 = Utilities.DistanceSquaredToLine(vec2.Zero, p1.swizzle.xz, p2.swizzle.xz, out var t);

			// The flat line is outside the flat circle.
			if (d1 > rSquared)
			{
				return false;
			}

			var result = p1 + (p2 - p1) * t;

			if (Math.Abs(result.y) <= halfHeight)
			{
				return true;
			}

			d1 = Utilities.DistanceSquaredToLine(halfVector, p1, p2, out _);
			d2 = Utilities.DistanceSquaredToLine(-halfVector, p1, p2, out _);

			// The line passes through one of the spherical caps.
			return d1 <= rSquared || d2 <= rSquared;
		}

		private static bool Overlaps(Capsule3D capsule, Sphere sphere)
		{
			// TODO: Finish this implementation.
			return false;
		}

		private static bool Overlaps(Cylinder cylinder, Shape3D other)
		{
			switch (other.Type)
			{
				case ShapeTypes3D.Cylinder: return Overlaps(cylinder, (Cylinder)other);
				case ShapeTypes3D.Line: return Overlaps(cylinder, (Line3D)other);
				case ShapeTypes3D.Sphere: return Overlaps(cylinder, (Sphere)other);
			}

			return false;
		}

		private static bool Overlaps(Cylinder cylinder1, Cylinder cylinder2)
		{
			var isOrientable1 = cylinder1.IsOrientable;
			var isOrientable2 = cylinder2.IsOrientable;

			// Neither shape is orientable.
			if (!isOrientable1 && !isOrientable2)
			{
				var p1 = cylinder1.Position;
				var p2 = cylinder2.Position;
				var delta = Math.Abs(p1.y - p2.y);
				var sumHeight = (cylinder1.Height + cylinder2.Height) / 2;

				if (delta > sumHeight)
				{
					return false;
				}

				var sumRadii = cylinder1.Radius + cylinder2.Radius;

				return Utilities.DistanceSquared(p1.swizzle.xz, p2.swizzle.xz) <= sumRadii * sumRadii;
			}

			// TODO: Finish this implementation.
			return false;
		}

		private static bool Overlaps(Cylinder cylinder, Line3D line)
		{
			// TODO: Finish this implementation.
			return false;
		}

		private static bool Overlaps(Cylinder cylinder, Sphere sphere)
		{
			// TODO: Finish this implementation.
			return false;
		}

		private static bool Overlaps(FlatShape3D flatShape, Shape3D other)
		{
			var flat = flatShape.FlatShape;

			// This means that the flat shape was never assigned (meaning there's definitely no overlap).
			if (flat == null)
			{
				return false;
			}

			// TODO: Finish this implementation.
			switch (flat.Type)
			{
			}

			return false;
		}

		private static bool Overlaps(Line3D line, Shape3D other)
		{
			switch (other.Type)
			{
				case ShapeTypes3D.Line: return Overlaps(line, (Line3D)other);
				case ShapeTypes3D.Sphere: return Overlaps(line, (Sphere)other);
			}

			return false;
		}

		private static bool Overlaps(Line3D line1, Line3D line2)
		{
			// TODO: Finish this implementation.
			return false;
		}

		private static bool Overlaps(Line3D line, Sphere sphere)
		{
			// TODO: Finish this implementation.
			return false;
		}

		private static bool Overlaps(Sphere sphere1, Sphere sphere2)
		{
			var squared = Utilities.DistanceSquared(sphere1.Position, sphere2.Position);
			var sum = sphere1.Radius + sphere2.Radius;

			return squared <= sum * sum;
		}
	}
}
