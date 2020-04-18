using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Shapes._3D
{
	public enum ShapeTypes3D
	{
		// Assigning values directly probably isn't necessary, but it's safer. Flat is intentionally put first since it
		// simplifies overlap functions in the shape helper.
		Box = 1,
		Capsule = 2,
		Cylinder = 3,
		Flat = 0,
		Line = 4,
		Point = 5,
		Sphere = 6
	}
}
