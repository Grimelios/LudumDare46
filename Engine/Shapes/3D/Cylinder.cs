﻿using System;
using Engine.Utility;
using GlmSharp;

namespace Engine.Shapes._3D
{
	public class Cylinder : Shape3D
	{
		public Cylinder() : this(0, 0)
		{
		}

		public Cylinder(float height, float radius, bool isOrientable = true) :
			base(ShapeTypes3D.Cylinder, isOrientable)
		{
			Height = height;
			Radius = radius;
			IsOrientable = isOrientable;
		}

		public float Height { get; set; }
		public float Radius { get; set; }

		protected override bool Contains(vec3 p)
		{
			if (!IsOrientable)
			{
				p = InverseOrientation * p;
			}

			if (Math.Abs(p.y) > Height / 2)
			{
				return false;
			}
			
			return Utilities.LengthSquared(p.swizzle.xz) <= Radius * Radius;
		}
	}
}
