using GlmSharp;

namespace Engine.Shapes._3D
{
	public class Point3D : Shape3D
	{
		public Point3D() : this(vec3.Zero)
		{
		}

		public Point3D(vec3 p) : base(ShapeTypes3D.Point, false)
		{
			Position = p;
		}

		protected override bool Contains(vec3 p)
		{
			return p == vec3.Zero;
		}
	}
}
